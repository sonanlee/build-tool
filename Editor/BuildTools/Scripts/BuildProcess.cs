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
        private const string BuildEntryNameArg = "-buildEntryName";
        private const string BuildFilePathArg = "-buildSetupPath";
        private const string BuildNumberArg = "-buildNumber";
        
        public static void Build(BuildSetup buildSetup, string buildEntryName="", int buildNumber = 0)
        {
            var defaultScenes = ScenesUtils.GetDefaultScenesAsArray();
            var playerSettingsSnapshot = new PlayerSettingsSnapshot();

            var setupList = buildSetup.entriesList;
            foreach (var setup in setupList)
            {
                if (string.IsNullOrEmpty(buildEntryName))
                {
                    if (setup.enabled)
                    {
                        BuildEntry(buildSetup, setup, playerSettingsSnapshot, defaultScenes, buildNumber);
                    }   
                }
                else if(buildEntryName == setup.buildName)
                {
                    BuildEntry(buildSetup, setup, playerSettingsSnapshot, defaultScenes, buildNumber);
                }
                else
                {
                    Debug.Log("Skipping Build " + setup.buildName);
                }
            }
        }

        private static void BuildEntry(BuildSetup buildSetup, BuildSetupEntry setup, PlayerSettingsSnapshot playerSettingsSnapshot, string[] defaultScenes, int buildNumber )
        {
            var path = buildSetup.exportDirectory;
            var target = setup.target;
            var targetGroup = BuildPipeline.GetBuildTargetGroup((BuildTarget)target);

            playerSettingsSnapshot.TakeSnapshot(targetGroup);

            PlayerSettings.SetScriptingBackend(targetGroup, setup.scriptingBackend);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, $"{buildSetup.commonScriptingDefineSymbols},{setup.scriptingDefineSymbols}");
            PlayerSettings.SetManagedStrippingLevel(targetGroup, setup.strippingLevel);

            // VR
            XRSettings.enabled = VRUtils.TargetGroupSupportsVirtualReality(targetGroup) && setup.supportsVR;

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

            if (setup.buildClient)
            {
                var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                var buildSummary = report.summary;
                var success = buildSummary.result == BuildResult.Succeeded;
                Debug.Log("Build " + setup.buildName + " ended with Status: " + buildSummary.result);

                if (!success && buildSetup.abortBatchOnFailure)
                {
                    Debug.LogError("Failure - Aborting remaining builds from batch");
                }
            }

            // Revert group build player settings after building
            playerSettingsSnapshot.ApplySnapshot();

        }
        private static void Build(string buildSetupRelativePath, string buildEntryName, int buildNumber = 0)
        {
            var buildSetup = AssetDatabase.LoadAssetAtPath(buildSetupRelativePath, typeof(BuildSetup)) as BuildSetup;
            if (buildSetup != null)
            {
                Build(buildSetup, buildEntryName, buildNumber);
            }
            else
            {
                Debug.LogError("Cannot find build setup in path: " + buildSetupRelativePath);
            }
        }

        public static void BuildWithArgs()
        {
            var buildFilePath = CLIUtils.GetCommandLineArg(BuildFilePathArg);
            var buildEntryName = CLIUtils.GetCommandLineArg(BuildEntryNameArg);
            var buildNumberStr = CLIUtils.GetCommandLineArg(BuildNumberArg);
            var buildNumber = 0;
            if (!string.IsNullOrEmpty(buildNumberStr))
            {
                buildNumber = int.Parse(buildNumberStr);
            }
            if (!string.IsNullOrEmpty(buildFilePath))
            {
                Build(buildFilePath, buildEntryName, buildNumber);
            }
            else
            {
                Debug.LogError("Cannot find build setup path, make sure to specify using " + BuildFilePathArg);
            }
        }

    }
}
