using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace Soma.Build
{
    public class EnhancedBuildsWindow : EditorWindow
    {
        private const string EDITOR_PREFS_KEY = "ObjectPath";
        private const string WINDOW_TITLE = "Enhanced Builds";
        public BuildSetup buildSetup;
        private Vector2 buildEntriesListScrollPos;

        [MenuItem("Builds/Open Enhanced Builds %#e")]
        static void Init()
        {
            EnhancedBuildsWindow window = (EnhancedBuildsWindow)EditorWindow.GetWindow(typeof(EnhancedBuildsWindow), false, WINDOW_TITLE, true);

            window.Show();
        }

        void OnEnable()
        {
            Undo.undoRedoPerformed += onUndoRedo;

            if (EditorPrefs.HasKey(EDITOR_PREFS_KEY))
            {
                string objectPath = EditorPrefs.GetString(EDITOR_PREFS_KEY);
                buildSetup = AssetDatabase.LoadAssetAtPath(objectPath, typeof(BuildSetup)) as BuildSetup;
            }
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= onUndoRedo;
        }

        private void onUndoRedo()
        {
            if (buildSetup)
            {
                EditorUtility.SetDirty(buildSetup);
                Repaint();
            }
        }

        void OnGUI()
        {
            EditorGUIUtility.labelWidth = 0f;

            GUILayout.Label("Build Setup Editor", EditorStyles.boldLabel);
            GUILayout.Space(10);
            if (buildSetup != null)
            {
                string objectPath = EditorPrefs.GetString(EDITOR_PREFS_KEY);
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
                selectBuildFile();
            }

            if (GUILayout.Button("Create New Build File"))
            {
                createNewBuildSetup();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (buildSetup != null)
            {
                GUILayout.Label("Loaded Build Setup", EditorStyles.boldLabel);

                GUILayout.Space(10);

                EditorGUIUtility.labelWidth = 200f;
                if (GUILayout.Button("Choose Root Directory", GUILayout.ExpandWidth(false)))
                {
                    Undo.RecordObject(buildSetup, "Set Build Setup Root Directory");
                    buildSetup.rootDirectory = EditorUtility.SaveFolderPanel("Choose Location", "", "");
                }
                EditorGUILayout.LabelField("Root Directory", buildSetup.rootDirectory);

                GUILayout.Space(10); 

                buildSetup.abortBatchOnFailure = EditorGUILayout.Toggle("Abort batch on failure", buildSetup.abortBatchOnFailure);

                int buildsAmount = buildSetup.entriesList.Count;

                GUILayout.Space(10);
                GUILayout.Label("Builds (" + buildsAmount + ")", EditorStyles.label);
                GUILayout.Space(5);

                if (buildsAmount > 0)
                {
                    buildEntriesListScrollPos = EditorGUILayout.BeginScrollView(buildEntriesListScrollPos, false, false, GUILayout.Width(position.width), GUILayout.MaxHeight(500));

                    var list = buildSetup.entriesList;
                    for (var i = 0; i < list.Count; i++)
                    {
                        var b = list[i];
                        EditorGUILayout.BeginHorizontal();

                        b.enabled = EditorGUILayout.Toggle("", b.enabled, GUILayout.MaxWidth(15.0f));
                        b.guiShowOptions = EditorGUILayout.Foldout(b.guiShowOptions, b.buildName, EditorStyles.foldout);

                        using (new EditorGUI.DisabledScope(i == 0))
                        {
                            if (GUILayout.Button(new GUIContent("↑", "Rearranges Build Entry up"), GUILayout.ExpandWidth(false)))
                            {
                                Undo.RecordObject(buildSetup, "Rearranged Build Entry up");
                                buildSetup.rearrangeBuildSetupEntry(b, true);
                            }
                        }

                        using (new EditorGUI.DisabledScope(i == list.Count - 1))
                        {
                            if (GUILayout.Button(new GUIContent("↓", "Rearranges Build Entry down"), GUILayout.ExpandWidth(false)))
                            {
                                Undo.RecordObject(buildSetup, "Rearranged Build Entry down");
                                buildSetup.rearrangeBuildSetupEntry(b, false);
                            }
                        }

                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button(new GUIContent("x", "Deletes Build Entry"), GUILayout.ExpandWidth(false)))
                        {
                            if(EditorUtility.DisplayDialog("Delete Build Entry?",
                                "Are you sure you want to delete build entry " + b.buildName
                                , "Yes", "No"))
                            {
                                Undo.RecordObject(buildSetup, "Removed Build Setup Entry");
                                buildSetup.deleteBuildSetupEntry(b);
                            }
                        }

                        GUI.backgroundColor = Color.green;
                        if (GUILayout.Button(new GUIContent("c", "Duplicates Build Entry"), GUILayout.ExpandWidth(false)))
                        {
                            Undo.RecordObject(buildSetup, "Duplicate Build Setup Entry");
                            buildSetup.duplicateBuildSetupEntry(b);
                        }
                        GUI.backgroundColor = Color.white;

                        EditorGUILayout.EndHorizontal();
                        if (b.guiShowOptions)
                        {
                            EditorGUI.indentLevel++;
                            drawBuildEntryGUI(b);
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
                    buildSetup.addBuildSetupEntry();
                }

                GUILayout.Space(10);

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                GUILayout.Space(10);

                var isReady = buildSetup.isReady();
                using (new EditorGUI.DisabledScope(!isReady))
                {
                    if (GUILayout.Button("Build", GUILayout.ExpandWidth(true)))
                    {
                        buildGame();
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

        private void drawBuildEntryGUI(BuildSetupEntry b)
        {
            b.buildName = EditorGUILayout.TextField("Build Name", b.buildName);
            b.target = (BuildTarget)EditorGUILayout.EnumPopup("Target", b.target);
            if(b.target > 0)
            {
                b.debugBuild = EditorGUILayout.Toggle("Debug Build", b.debugBuild);
                b.scriptingDefineSymbols = EditorGUILayout.TextField("Scripting Define Symbols", b.scriptingDefineSymbols);

                drawScenesSectionGUI(b);
                drawAdvancedOptionsSectionGUI(b);
                drawVRSectionGUI(b);
            }
        }

        private void drawScenesSectionGUI(BuildSetupEntry b)
        {
            b.useDefaultBuildScenes = EditorGUILayout.Toggle("Use Default Build Scenes", b.useDefaultBuildScenes);

            if (!b.useDefaultBuildScenes)
            {
                b.guiShowCustomScenes = EditorGUILayout.Foldout(b.guiShowCustomScenes, "Custom Scenes");
                if (b.guiShowCustomScenes)
                {
                    EditorGUI.indentLevel++;
                    if (b.customScenes.Count > 0)
                    {
                        var scenes = b.customScenes;

                        for (int i = 0; i < scenes.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            scenes[i] = EditorGUILayout.TextField("Scene " + i, scenes[i]);
                            if (GUILayout.Button("Select Scene", GUILayout.ExpandWidth(false)))
                            {
                                string absPath = EditorUtility.OpenFilePanel("Select Scene file", "", "unity");
                                if (absPath.StartsWith(Application.dataPath))
                                {
                                    string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
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

                    using(var horizontalScope = new GUILayout.HorizontalScope())
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

        private void drawAdvancedOptionsSectionGUI(BuildSetupEntry b)
        {
            b.guiShowAdvancedOptions = EditorGUILayout.Foldout(b.guiShowAdvancedOptions, "Advanced Options");
            if (b.guiShowAdvancedOptions)
            {
                EditorGUI.indentLevel++;

#if UNITY_2020_1_OR_NEWER
                b.detailedBuildReport = EditorGUILayout.Toggle("Detailed Build Report", b.detailedBuildReport);
#endif

                b.rebuildAddressables = EditorGUILayout.Toggle("Rebuild Addressables", b.rebuildAddressables);

#if UNITY_2018_3_OR_NEWER
                b.strippingLevel = (ManagedStrippingLevel)EditorGUILayout.EnumPopup("Stripping Level", b.strippingLevel);
#endif

                b.strictMode = EditorGUILayout.Toggle(new GUIContent("Strict Mode",
                                                    "Do not allow the build to succeed if any errors are reported."),
                                                    b.strictMode);
                b.assetBundleManifestPath = EditorGUILayout.TextField("AssetBundle Manifest Path", b.assetBundleManifestPath);
                b.scriptingBackend = (ScriptingImplementation)EditorGUILayout.EnumPopup("Scripting Backend", b.scriptingBackend);

                if (b.target == BuildTarget.iOS)
                {
                    b.iosSymlinkLibraries = EditorGUILayout.Toggle("XCode - Symlink Library", b.iosSymlinkLibraries);
                }

                if (b.target == BuildTarget.Android)
                {
                    #if UNITY_2017_4_OR_NEWER
                    b.androidAppBundle = EditorGUILayout.Toggle("Build Android App Bundle", b.androidAppBundle);
                    b.androidArchitecture = (AndroidArchitecture)EditorGUILayout.EnumPopup("Android Architecture", b.androidArchitecture);
                    #endif
                }

                if(b.target == BuildTarget.PS4)
                {
                    b.ps4BuildSubtarget = (PS4BuildSubtarget)EditorGUILayout.EnumPopup("PS4 Build Subtarget", b.ps4BuildSubtarget);
                    b.ps4HardwareTarget = (PS4HardwareTarget)EditorGUILayout.EnumPopup("PS4 Hardware Target", b.ps4HardwareTarget);
                }

                EditorGUI.indentLevel--;
            }
        }

        private void drawVRSectionGUI(BuildSetupEntry b)
        {
            #if UNITY_2017_2_OR_NEWER
            var targetGroup = BuildPipeline.GetBuildTargetGroup(b.target);
            if(VRUtils.targetGroupSupportsVirtualReality(targetGroup))
            {
                b.supportsVR = EditorGUILayout.Toggle("VR Support", b.supportsVR);
                if (b.supportsVR)
                {
                    b.guiShowVROptions = EditorGUILayout.Foldout(b.guiShowVROptions, "VR Options");
                    if (b.guiShowVROptions)
                    {
                        EditorGUI.indentLevel++;

                        var vrSdks = VRUtils.getAvailableVRSdks(targetGroup);
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
            #endif
        }

        private void buildGame()
        {
            BuildProcess.Build(buildSetup);
        }

        private void createNewBuildSetup()
        {
            buildSetup = BuildSetup.Create();
            if (buildSetup)
            {
                buildSetup.entriesList = new List<BuildSetupEntry>();
                string relPath = AssetDatabase.GetAssetPath(buildSetup);
                EditorPrefs.SetString(EDITOR_PREFS_KEY, relPath);
            }
        }
        private void selectBuildFile()
        {
            string absPath = EditorUtility.OpenFilePanel("Select Build Setup file", BuildUtils.SETUPS_REL_DIRECTORY, "asset");
            if (absPath.StartsWith(Application.dataPath))
            {
                string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
                var loadedBuildAsset = AssetDatabase.LoadAssetAtPath(relPath, typeof(BuildSetup)) as BuildSetup;

                if (loadedBuildAsset)
                {
                    buildSetup = loadedBuildAsset;
                    EditorPrefs.SetString(EDITOR_PREFS_KEY, relPath);
                }
            }
        }


    }
}