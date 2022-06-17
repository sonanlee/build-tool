using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
#if UNITY_EDITOR_OSX
using UnityEditor.OSXStandalone;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.XR;

namespace Soma.Build
{
    public static class BuildProcess
    {
        private const string BuildEntryNameArg = "-buildEntryName";
        private const string BuildFilePathArg = "-buildSetupPath";
        private const string BuildExportPathArg = "-buildExportPath";
        private const string BuildNumberArg = "-buildNumber";
        private const string ImportPackageArg = "-importPackage";
        public static int Build(BuildSetup buildSetup, string buildEntryName="", string buildExportPath = "", int buildNumber = 0)
        {
            var defaultScenes = ScenesUtils.GetDefaultScenesAsArray();
            var playerSettingsSnapshot = new PlayerSettingsSnapshot();

            var setupList = buildSetup.entriesList;
            var errorCode = 0;
            foreach (var setup in setupList)
            {
                if (string.IsNullOrEmpty(buildEntryName))
                {
                    if (setup.enabled)
                    {
                        var error = BuildEntry(buildSetup, setup, playerSettingsSnapshot, buildExportPath, defaultScenes, buildNumber);
                        if (error != 0)
                        {
                            errorCode = -1;
                        }
                    }   
                }
                else if(buildEntryName == setup.buildName)
                {
                    var error = BuildEntry(buildSetup, setup, playerSettingsSnapshot, buildExportPath, defaultScenes, buildNumber);
                    if (error != 0)
                    {
                        errorCode = -1;
                    }
                }
            }

            return errorCode;
        }

        private static int BuildEntry(BuildSetup buildSetup, BuildSetupEntry setup, PlayerSettingsSnapshot playerSettingsSnapshot, string buildExportPath, string[] defaultScenes, int buildNumber )
        {
            var path = string.IsNullOrEmpty(buildExportPath) ? buildSetup.exportDirectory : buildExportPath;
            var target = setup.target;
            var targetGroup = BuildPipeline.GetBuildTargetGroup((BuildTarget)target);

            playerSettingsSnapshot.TakeSnapshot(targetGroup);

            PlayerSettings.SetScriptingBackend(targetGroup, setup.scriptingBackend);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, $"{buildSetup.commonScriptingDefineSymbols};{setup.scriptingDefineSymbols}");
            PlayerSettings.SetManagedStrippingLevel(targetGroup, setup.strippingLevel);
            PlayerSettings.productName = setup.productName;
            PlayerSettings.SetApplicationIdentifier(targetGroup, $"com.{PlayerSettings.companyName}.{PlayerSettings.productName.ToLower()}");

            // VR
            XRSettings.enabled = VRUtils.TargetGroupSupportsVirtualReality(targetGroup) && setup.supportsVR;

            // Android
            if (target == SomaBuildTarget.Android)
            {
                EditorUserBuildSettings.buildAppBundle = setup.androidAppBundle;
                PlayerSettings.Android.targetArchitectures = (AndroidArchitecture)setup.androidArchitecture;
                
                //Key Chain Password
                PlayerSettings.Android.useCustomKeystore = true;

                PlayerSettings.Android.keystoreName= "soma.keystore";
                PlayerSettings.Android.keystorePass = "somadevelopment";

                PlayerSettings.Android.keyaliasName = "soma";
                PlayerSettings.Android.keyaliasPass = "somadevelopment";
                
            }

#if UNITY_EDITOR_OSX
            if (target == SomaBuildTarget.MacOS)
            {
                // MacOS Mono 로 고정
                // PlayerSettings.SetScriptingBackend(targetGroup, ScriptingImplementation.Mono2x);
                UserBuildSettings.architecture = (MacOSArchitecture)setup.macOSArchitecture;
            }
#endif

            if (setup.buildAddressables)
            {
                Debug.Log($"[Addressable Build] Profile : {setup.profileNameAddressable}");
                AddressableAssetSettingsDefaultObject.Settings.activeProfileId = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetProfileId(setup.profileNameAddressable);
                if (string.IsNullOrEmpty(AddressableAssetSettingsDefaultObject.Settings.activeProfileId))
                {
                    Debug.LogError($"Profile doesn't exist : {setup.profileNameAddressable}");
                    return -1;
                }
                if (setup.contentOnlyBuild)
                {
                    var contentStateBinPathAddressable = $"Assets/AddressableAssetsData/{PlatformMappingService.GetPlatformPathSubFolder()}/addressables_content_state.bin";
                    if (!string.IsNullOrEmpty(contentStateBinPathAddressable))
                    {
                        var result = ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, contentStateBinPathAddressable);
                        if (!string.IsNullOrEmpty(result.Error))
                        {
                            Debug.LogError($"[Addressable Build] Error : {result.Error}");
                            return -1;
                        }
                    }
                    else
                    {
                        Debug.LogError("Addressable Content-State-Bin File is Empty");
                        return -1;
                    }
                }
                else
                {
                    Debug.Log($"[Addressable Build] ClearPlayerContent");
                    AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
                    Debug.Log($"[Addressable Build] BuildPlayerContent");
                    AddressableAssetSettings.BuildPlayerContent(out var result);
                    if (!string.IsNullOrEmpty(result.Error))
                    {
                        Debug.LogError($"[Addressable Build] Error : {result.Error}");
                        return -1;
                    }
                }
            }

            // Common Process
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.Android.bundleVersionCode = buildNumber;
            PlayerSettings.iOS.buildNumber = buildNumber.ToString();
            PlayerSettings.bundleVersion = $"{PlayerSettings.bundleVersion}.{buildNumber}";
            
            var buildPlayerOptions = BuildUtils.GetBuildPlayerOptionsFromBuildSetupEntry(setup, path, defaultScenes);

            if (setup.buildClient)
            {
                var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                var buildSummary = report.summary;
                var success = buildSummary.result == BuildResult.Succeeded;
                Debug.Log($"[Build] {setup.buildName} ended with Status: {buildSummary.result}");
                Debug.Log($"[Build Summary] Error : {buildSummary.totalErrors}, Warning : {buildSummary.totalWarnings}");

                if (!success)
                {
                    Debug.Log("[Build] Printing All Errors");
                    foreach (var step in report.steps)
                    {
                        foreach (var msg in step.messages)
                        {
                            if (msg.type == LogType.Error || msg.type == LogType.Exception)
                            {
                                Debug.Log($"[Build {msg.type.ToString()}][{step.name}] {msg.content}");
                            }
                        }
                    }
                    playerSettingsSnapshot.ApplySnapshot();
                    return -1;
                }
            }
            playerSettingsSnapshot.ApplySnapshot();
            // Revert group build player settings after building
            return 0;

        }
        private static int Build(string buildSetupRelativePath, string buildEntryName, string buildExportPath, int buildNumber = 0)
        {
            var buildSetup = AssetDatabase.LoadAssetAtPath(buildSetupRelativePath, typeof(BuildSetup)) as BuildSetup;
            if (buildSetup != null)
            {
                return Build(buildSetup, buildEntryName, buildExportPath, buildNumber);
            }
            else
            {
                Debug.LogError("Cannot find build setup in path: " + buildSetupRelativePath);
                return -1;
            }
        }

        public static void BuildWithArgs()
        {
            var buildFilePath = CLIUtils.GetCommandLineArg(BuildFilePathArg);
            var buildEntryName = CLIUtils.GetCommandLineArg(BuildEntryNameArg);
            var buildNumberStr = CLIUtils.GetCommandLineArg(BuildNumberArg);
            var buildExportPath = CLIUtils.GetCommandLineArg(BuildExportPathArg);
            var buildNumber = 0;
            
            if (!string.IsNullOrEmpty(buildNumberStr))
            {
                buildNumber = int.Parse(buildNumberStr);
            }

            
            Debug.Log($"[Build] BuildWithArgs with {buildEntryName} from {buildFilePath}");
            if (!string.IsNullOrEmpty(buildFilePath))
            {
                var errorCode = Build(buildFilePath, buildEntryName, buildExportPath, buildNumber);
                if (errorCode != 0)
                {
                    Debug.LogError("[Build] Build Fail");
                    EditorApplication.Exit(-1);
                }
            }
            else
            {
                Debug.LogError("Cannot find build setup path, make sure to specify using " + BuildFilePathArg);
                EditorApplication.Exit(-1);

            }
            
            EditorApplication.Exit(0);
        }

        public static void AddressableBuildWithArgs()
        {
            var buildFilePath = CLIUtils.GetCommandLineArg(BuildFilePathArg);
            var buildEntryName = CLIUtils.GetCommandLineArg(BuildEntryNameArg);
            Debug.Log($"[Addressable Build] Build Start with {buildEntryName} from {buildFilePath}");
            
            if (!string.IsNullOrEmpty(buildFilePath))
            {
                var errorCode = BuildAddressable(buildFilePath, buildEntryName);
                if (errorCode != 0)
                {
                    Debug.LogError("[Addressable Build] Build Fail");
                    EditorApplication.Exit(-1);
                }
            }
            else
            {
                Debug.LogError("Cannot find build setup path, make sure to specify using " + BuildFilePathArg);
                EditorApplication.Exit(-1);

            }
            
            EditorApplication.Exit(0);
        }


        private static int BuildAddressable(string buildSetupRelativePath, string buildEntryName)
        {
            var buildSetup = AssetDatabase.LoadAssetAtPath(buildSetupRelativePath, typeof(BuildSetup)) as BuildSetup;
            if (buildSetup != null)
            {
                var setupList = buildSetup.entriesList;
                var errorCode = 0;
                foreach (var setup in setupList)
                {
                    if (string.IsNullOrEmpty(buildEntryName))
                    {
                        continue;
                    }

                    if (!setup.enabled || buildEntryName != setup.buildName)
                    {
                        continue;
                    }

                    if (setup.buildAddressables)
                    {
                        Debug.Log($"[Addressable Build] Profile : {setup.profileNameAddressable}");
                        
                        AddressableAssetSettingsDefaultObject.Settings.activeProfileId = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetProfileId(setup.profileNameAddressable);
                        if (string.IsNullOrEmpty(AddressableAssetSettingsDefaultObject.Settings.activeProfileId))
                        {
                            Debug.LogError($"Profile doesn't exist : {setup.profileNameAddressable}");
                            errorCode = -1;
                        }
                        if (setup.contentOnlyBuild)
                        {
                            var contentStateBinPathAddressable = $"Assets/AddressableAssetsData/{PlatformMappingService.GetPlatformPathSubFolder()}/addressables_content_state.bin";
                            if (!string.IsNullOrEmpty(contentStateBinPathAddressable))
                            {
                                var result = ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, contentStateBinPathAddressable);
                                if (!string.IsNullOrEmpty(result.Error))
                                {
                                    Debug.LogError($"[Addressable Build] Error : {result.Error}");
                                    errorCode = -1;
                                }
                            }
                            else
                            {
                                Debug.LogError("Addressable Content-State-Bin File is Empty");
                                errorCode = -1;
                            }
                        }
                        else
                        {
                            Debug.Log($"[Addressable Build] ClearPlayerContent");
                            AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
                            Debug.Log($"[Addressable Build] BuildPlayerContent");
                            AddressableAssetSettings.BuildPlayerContent(out var result);
                            if (!string.IsNullOrEmpty(result.Error))
                            {
                                Debug.LogError($"[Addressable Build] Error : {result.Error}");
                                errorCode = -1;
                            }
                        }
                    }
                }

                return errorCode;
            }
            else
            {
                Debug.LogError("Cannot find build setup in path: " + buildSetupRelativePath);
                return -1;
            }
        }
        
    }
}
