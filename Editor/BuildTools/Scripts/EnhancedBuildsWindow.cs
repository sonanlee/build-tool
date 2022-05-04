using System.Collections.Generic;
using UnityEditor;

#if UNITY_EDITOR_OSX
using UnityEditor.OSXStandalone;
#endif

using UnityEngine;

namespace Soma.Build
{
    public class EnhancedBuildsWindow : EditorWindow
    {
        private const string EditorPrefsKey = "ObjectPath";
        private const string WindowTitle = "Enhanced Builds";
        public BuildSetup buildSetup;
        private Vector2 _buildEntriesListScrollPos;

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;

            if (EditorPrefs.HasKey(EditorPrefsKey))
            {
                var objectPath = EditorPrefs.GetString(EditorPrefsKey);
                buildSetup = AssetDatabase.LoadAssetAtPath(objectPath, typeof(BuildSetup)) as BuildSetup;
            }
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 0f;

            GUILayout.Label("Build Setup Editor", EditorStyles.boldLabel);
            GUILayout.Space(10);
            if (buildSetup != null)
            {
                var objectPath = EditorPrefs.GetString(EditorPrefsKey);
                EditorGUILayout.LabelField("Current Build Setup File", objectPath);
            }

            GUILayout.BeginHorizontal();

            if (buildSetup != null)
            {
                if (GUILayout.Button("Show in Library"))
                {
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = buildSetup;
                }
            }

            if (GUILayout.Button("Select Build File"))
            {
                SelectBuildFile();
            }

            if (GUILayout.Button("Create New Build File"))
            {
                CreateNewBuildSetup();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (buildSetup != null)
            {
                GUILayout.Label("Loaded Build Setup", EditorStyles.boldLabel);

                EditorGUILayout.LabelField("Product Name", Application.productName);
                EditorGUILayout.LabelField("Company Name", Application.companyName);
                EditorGUILayout.LabelField("Product Version", PlayerSettings.bundleVersion);
                EditorGUILayout.LabelField("Unity Version", Application.unityVersion);

                GUILayout.Space(10);

                EditorGUIUtility.labelWidth = 200f;
                if (GUILayout.Button("Choose Export Directory", GUILayout.ExpandWidth(false)))
                {
                    Undo.RecordObject(buildSetup, "Set Build Setup Export Directory");
                    buildSetup.exportDirectory = EditorUtility.SaveFolderPanel("Choose Location", "", "");
                }

                EditorGUILayout.LabelField("Export Directory", buildSetup.exportDirectory);

                GUILayout.Space(10);

                buildSetup.abortBatchOnFailure = EditorGUILayout.Toggle("Abort batch on failure", buildSetup.abortBatchOnFailure);
                buildSetup.commonScriptingDefineSymbols = EditorGUILayout.TextField("Common Scripting Define Symbols", buildSetup.commonScriptingDefineSymbols);
                
                var buildsAmount = buildSetup.entriesList.Count;

                GUILayout.Space(10);
                GUILayout.Label("Builds (" + buildsAmount + ")", EditorStyles.label);
                GUILayout.Space(5);

                if (buildsAmount > 0)
                {
                    _buildEntriesListScrollPos = EditorGUILayout.BeginScrollView(_buildEntriesListScrollPos, false, false, GUILayout.Width(position.width), GUILayout.MaxHeight(500));

                    var list = buildSetup.entriesList;
                    for (var i = 0; i < list.Count; i++)
                    {
                        var b = list[i];
                        EditorGUILayout.BeginHorizontal();

                        b.enabled = EditorGUILayout.Toggle("", b.enabled, GUILayout.MaxWidth(15.0f));
                        b._guiShowOptions = EditorGUILayout.Foldout(b._guiShowOptions, b.buildName, EditorStyles.foldout);

                        using (new EditorGUI.DisabledScope(i == 0))
                        {
                            if (GUILayout.Button(new GUIContent("↑", "Rearranges Build Entry up"), GUILayout.ExpandWidth(false)))
                            {
                                Undo.RecordObject(buildSetup, "Rearranged Build Entry up");
                                buildSetup.RearrangeBuildSetupEntry(b, true);
                            }
                        }

                        using (new EditorGUI.DisabledScope(i == list.Count - 1))
                        {
                            if (GUILayout.Button(new GUIContent("↓", "Rearranges Build Entry down"), GUILayout.ExpandWidth(false)))
                            {
                                Undo.RecordObject(buildSetup, "Rearranged Build Entry down");
                                buildSetup.RearrangeBuildSetupEntry(b, false);
                            }
                        }

                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button(new GUIContent("x", "Deletes Build Entry"), GUILayout.ExpandWidth(false)))
                        {
                            if (EditorUtility.DisplayDialog("Delete Build Entry?",
                                    "Are you sure you want to delete build entry " + b.buildName
                                    , "Yes", "No"))
                            {
                                Undo.RecordObject(buildSetup, "Removed Build Setup Entry");
                                buildSetup.DeleteBuildSetupEntry(b);
                            }
                        }

                        GUI.backgroundColor = Color.yellow;
                        if (GUILayout.Button(new GUIContent("c", "Duplicates Build Entry"), GUILayout.ExpandWidth(false)))
                        {
                            Undo.RecordObject(buildSetup, "Duplicate Build Setup Entry");
                            buildSetup.DuplicateBuildSetupEntry(b);
                        }

                        GUI.backgroundColor = Color.green;
                        if (GUILayout.Button(new GUIContent("Build", "Build Entry"), GUILayout.ExpandWidth(false)))
                        {
                            BuildProcess.Build(buildSetup, b.buildName, null, 0);
                        }
                    
                        GUI.backgroundColor = Color.white;

                        EditorGUILayout.EndHorizontal();
                        if (b._guiShowOptions)
                        {
                            EditorGUI.indentLevel++;
                            DrawBuildEntryGUI(b);
                            EditorGUI.indentLevel--;
                        }

                        GUILayout.Space(5);
                    }

                    EditorGUILayout.EndScrollView();
                }
                else
                {
                    GUILayout.Label("This Built List is Empty");
                }

                GUILayout.Space(10);

                if (GUILayout.Button(new GUIContent("Add Entry", "Adds a new build entry to the list"), GUILayout.ExpandWidth(true)))
                {
                    Undo.RecordObject(buildSetup, "Add Build Setup Entry");
                    buildSetup.AddBuildSetupEntry();
                }

                GUILayout.Space(10);

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                GUILayout.Space(10);

                var isReady = buildSetup.IsReady();
                using (new EditorGUI.DisabledScope(!isReady))
                {
                    if (GUILayout.Button("Build All", GUILayout.ExpandWidth(true)))
                    {
                        BuildGame();
                    }
                }

                if (!isReady)
                {
                    GUILayout.Label("Define a Root directory and at least one active build entry");
                }
            }
            else
            {
                GUILayout.Label("Select or Create a new Build Setup", EditorStyles.boldLabel);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(buildSetup);
            }
        }

        [MenuItem("Builds/Open Enhanced Builds %#e")]
        private static void Init()
        {
            var window = (EnhancedBuildsWindow)GetWindow(typeof(EnhancedBuildsWindow), false, WindowTitle, true);

            window.Show();
        }

        private void OnUndoRedo()
        {
            if (buildSetup)
            {
                EditorUtility.SetDirty(buildSetup);
                Repaint();
            }
        }

        private void DrawBuildEntryGUI(BuildSetupEntry b)
        {
            b.buildName = EditorGUILayout.TextField("Build Name", b.buildName);
            b.target = (SomaBuildTarget)EditorGUILayout.EnumPopup("Target", b.target);
            if (b.target > 0)
            {
                b.buildClient = EditorGUILayout.Toggle("Build Client", b.buildClient);
                b.developmentBuild = EditorGUILayout.Toggle("Development Build", b.developmentBuild);
                b.scriptingDefineSymbols = EditorGUILayout.TextField("Scripting Define Symbols", b.scriptingDefineSymbols);

                DrawScenesSectionGUI(b);
                DrawAdvancedOptionsSectionGUI(b);
                DrawAddressableSectionGUI(b);
                DrawVRSectionGUI(b);
            }
        }

        private void DrawScenesSectionGUI(BuildSetupEntry b)
        {
            b.useDefaultBuildScenes = EditorGUILayout.Toggle("Use Default Build Scenes", b.useDefaultBuildScenes);

            if (!b.useDefaultBuildScenes)
            {
                b._guiShowCustomScenes = EditorGUILayout.Foldout(b._guiShowCustomScenes, "Custom Scenes");
                if (b._guiShowCustomScenes)
                {
                    EditorGUI.indentLevel++;
                    if (b.customScenes.Count > 0)
                    {
                        var scenes = b.customScenes;

                        for (var i = 0; i < scenes.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            scenes[i] = EditorGUILayout.TextField("Scene " + i, scenes[i]);
                            if (GUILayout.Button("Select Scene", GUILayout.ExpandWidth(false)))
                            {
                                var absPath = EditorUtility.OpenFilePanel("Select Scene file", "", "unity");
                                if (absPath.StartsWith(Application.dataPath))
                                {
                                    var relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
                                    scenes[i] = relPath;
                                }
                            }

                            if (GUILayout.Button("Remove Scene", GUILayout.ExpandWidth(false)))
                            {
                                Undo.RecordObject(buildSetup, "Remove Build Setup Entry Custom scene");
                                b.customScenes.RemoveAt(i);
                                i--;
                            }

                            GUILayout.EndHorizontal();
                        }
                    }

                    using (var horizontalScope = new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20f);

                        if (GUILayout.Button("Add Scene", GUILayout.ExpandWidth(false)))
                        {
                            Undo.RecordObject(buildSetup, "Add Build Setup Entry Custom scene");
                            b.customScenes.Add(string.Empty);
                        }
                    }


                    EditorGUI.indentLevel--;
                }
            }
        }

        private void DrawAdvancedOptionsSectionGUI(BuildSetupEntry b)
        {
            b._guiShowAdvancedOptions = EditorGUILayout.Foldout(b._guiShowAdvancedOptions, "Advanced Options");
            if (b._guiShowAdvancedOptions)
            {
                EditorGUI.indentLevel++;
                b.detailedBuildReport = EditorGUILayout.Toggle("Detailed Build Report", b.detailedBuildReport);
                b.strippingLevel = (ManagedStrippingLevel)EditorGUILayout.EnumPopup("Stripping Level", b.strippingLevel);
                b.strictMode = EditorGUILayout.Toggle(new GUIContent("Strict Mode",
                        "Do not allow the build to succeed if any errors are reported."),
                    b.strictMode);
                b.scriptingBackend = (ScriptingImplementation)EditorGUILayout.EnumPopup("Scripting Backend", b.scriptingBackend);

                if (b.target == SomaBuildTarget.iOS)
                {
                    b.iosSymlinkLibraries = EditorGUILayout.Toggle("XCode - Symlink Library", b.iosSymlinkLibraries);
                }

                if (b.target == SomaBuildTarget.Android)
                {
                    b.androidAppBundle = EditorGUILayout.Toggle("Build Android App Bundle", b.androidAppBundle);
                    b.androidArchitecture = (AndroidArchitecture)EditorGUILayout.EnumPopup("Android Architecture", b.androidArchitecture);
                }
                
                
#if UNITY_EDITOR_OSX
                if (b.target == SomaBuildTarget.MacOS)
                {
                    b.macOSArchitecture = (SomaMacOSArchitecture)EditorGUILayout.EnumPopup("MacOS Architecture", b.macOSArchitecture);
                }
#endif

                EditorGUI.indentLevel--;
            }
        }

        private void DrawVRSectionGUI(BuildSetupEntry b)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup((BuildTarget)b.target);
            if (VRUtils.TargetGroupSupportsVirtualReality(targetGroup))
            {
                b.supportsVR = EditorGUILayout.Toggle("VR Support", b.supportsVR);
                if (b.supportsVR)
                {
                    b._guiShowVROptions = EditorGUILayout.Foldout(b._guiShowVROptions, "VR Options");
                    if (b._guiShowVROptions)
                    {
                        EditorGUI.indentLevel++;

                        var vrSdks = VRUtils.GetAvailableVRSdks(targetGroup);
                        if (vrSdks.Length > 0)
                        {
                            b.vrSdkFlags = EditorGUILayout.MaskField("VR SDKs", b.vrSdkFlags, vrSdks);
                        }
                        else
                        {
                            GUILayout.Label("No VR SDK available for the current build target.");
                        }

                        EditorGUI.indentLevel--;
                    }
                }
            }
        }

        private void DrawAddressableSectionGUI(BuildSetupEntry b)
        {
            b._guiShowAddressableOptions = EditorGUILayout.Foldout(b._guiShowAddressableOptions, "Addressable Options");
            if (b._guiShowAddressableOptions)
            {
                EditorGUI.indentLevel++;
                b.buildAddressables = EditorGUILayout.Toggle("Build Addressables", b.buildAddressables);
                if (b.buildAddressables)
                {
                    b.contentOnlyBuild = EditorGUILayout.Toggle("Build ContentOnly", b.contentOnlyBuild);
                    if (b.contentOnlyBuild)
                    {
                        
                        GUILayout.BeginHorizontal();
                        b.contentStateBinPathAddressable = EditorGUILayout.TextField("StateBinFile File", b.contentStateBinPathAddressable);
                        if (GUILayout.Button("Choose"))
                        {
                            b.contentStateBinPathAddressable = EditorUtility.OpenFilePanel("Choose addressables_content_state.bin", buildSetup.exportDirectory, "bin");
                        }
                        GUILayout.EndHorizontal();
                    }
                    b.profileNameAddressable = EditorGUILayout.TextField("Profile Name", b.profileNameAddressable);
   
                }
                EditorGUI.indentLevel--;
            }
        }
        private void BuildGame()
        {
            BuildProcess.Build(buildSetup);
        }

        private void CreateNewBuildSetup()
        {
            buildSetup = BuildSetup.Create();
            if (buildSetup)
            {
                buildSetup.entriesList = new List<BuildSetupEntry>();
                var relPath = AssetDatabase.GetAssetPath(buildSetup);
                EditorPrefs.SetString(EditorPrefsKey, relPath);
            }
        }

        private void SelectBuildFile()
        {
            var absPath = EditorUtility.OpenFilePanel("Select Build Setup file", BuildUtils.SetupsDirectory, "asset");
            if (absPath.StartsWith(Application.dataPath))
            {
                var relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
                var loadedBuildAsset = AssetDatabase.LoadAssetAtPath(relPath, typeof(BuildSetup)) as BuildSetup;

                if (loadedBuildAsset)
                {
                    buildSetup = loadedBuildAsset;
                    EditorPrefs.SetString(EditorPrefsKey, relPath);
                }
            }
        }
    }
}
