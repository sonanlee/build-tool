using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEditor.OSXStandalone;
using UnityEngine;
using UnityEngine.XR;

namespace Soma.Build
{
    public static class BuildProcess
    {
        private const string BuildFilePathArg = "-buildSetupPath";
        private const string BuildNumberArg = "-buildNumber";
        
        public static void Build(BuildSetup buildSetup, int buildNumber = 0)
        {
            var defaultScenes = ScenesUtils.GetDefaultScenesAsArray();

            var path = buildSetup.exportDirectory;

            var playerSettingsSnapshot = new PlayerSettingsSnapshot();

            var setupList = buildSetup.entriesList;
            foreach (var setup in setupList)
            {
                if (setup.enabled)
                {
                    var target = setup.target;
                    var targetGroup = BuildPipeline.GetBuildTargetGroup((BuildTarget)target);

                    playerSettingsSnapshot.TakeSnapshot(targetGroup);

                    PlayerSettings.SetScriptingBackend(targetGroup, setup.scriptingBackend);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, $"{buildSetup.commonScriptingDefineSymbols},{setup.scriptingDefineSymbols}");
                    PlayerSettings.SetManagedStrippingLevel(targetGroup, setup.strippingLevel);

                    // VR
                    if (VRUtils.TargetGroupSupportsVirtualReality(targetGroup))
                    {
                        XRSettings.enabled = setup.supportsVR;
                    }
                    else
                    {
                        XRSettings.enabled = false;
                    }

                    // Android
                    if (target == SomaBuildTarget.Android)
                    {
                        EditorUserBuildSettings.buildAppBundle = setup.androidAppBundle;
                        PlayerSettings.Android.targetArchitectures = setup.androidArchitecture;
                    }

#if UNITY_EDITOR_OSX
                    if (target == SomaBuildTarget.MacOS)
                    {
                        UserBuildSettings.architecture = (MacOSArchitecture)setup.macOSArchitecture;
                    }
#endif

                    if (setup.buildAddressables)
                    {
                        AddressableAssetSettingsDefaultObject.Settings.activeProfileId = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetProfileId(setup.profileNameAddressable);
                        if (setup.contentOnlyBuild)
                        {
                            if (!string.IsNullOrEmpty(setup.contentStateBinPathAddressable))
                            {
                                ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, setup.contentStateBinPathAddressable);
                            }
                            else
                            {
                                Debug.LogError("Addressable Content-State-Bin File is Empty");
                            }
                        }
                        else
                        {
                            AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
                            AddressableAssetSettings.BuildPlayerContent();
                        }
                    }

                    // Common Process
                    PlayerSettings.SplashScreen.show = false;
                    PlayerSettings.SplashScreen.showUnityLogo = false;
                    PlayerSettings.Android.bundleVersionCode = buildNumber;
                    PlayerSettings.iOS.buildNumber = buildNumber.ToString();
                    PlayerSettings.bundleVersion = $"{PlayerSettings.bundleVersion}.{buildNumber}";
                    
                    var buildPlayerOptions = BuildUtils.GetBuildPlayerOptionsFromBuildSetupEntry(setup, path, defaultScenes);
                    
                    var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                    var buildSummary = report.summary;
                    var success = buildSummary.result == BuildResult.Succeeded;
                    Debug.Log("Build " + setup.buildName + " ended with Status: " + buildSummary.result);

                    // Revert group build player settings after building
                    playerSettingsSnapshot.ApplySnapshot();

                    if (!success && buildSetup.abortBatchOnFailure)
                    {
                        Debug.LogError("Failure - Aborting remaining builds from batch");
                        break;
                    }
                }
                else
                {
                    Debug.Log("Skipping Build " + setup.buildName);
                }
            }
        }

        public static void Build(string buildSetupRelativePath, int buildNumber = 0)
        {
            var buildSetup = AssetDatabase.LoadAssetAtPath(buildSetupRelativePath, typeof(BuildSetup)) as BuildSetup;
            if (buildSetup != null)
            {
                Build(buildSetup, buildNumber);
            }
            else
            {
                Debug.LogError("Cannot find build setup in path: " + buildSetupRelativePath);
            }
        }

        public static void BuildWithArgs()
        {
            var buildFilePath = CLIUtils.GetCommandLineArg(BuildFilePathArg);
            var buildNumberStr = CLIUtils.GetCommandLineArg(BuildNumberArg);
            var buildNumber = 0;
            if (!string.IsNullOrEmpty(buildNumberStr))
            {
                buildNumber = int.Parse(buildNumberStr);
            }
            if (!string.IsNullOrEmpty(buildFilePath))
            {
                Build(buildFilePath, buildNumber);
            }
            else
            {
                Debug.LogError("Cannot find build setup path, make sure to specify using " + BuildFilePathArg);
            }
        }
    }
}
