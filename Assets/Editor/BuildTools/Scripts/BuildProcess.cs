using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEditor.OSXStandalone;
using UnityEngine;
using UnityEngine.XR;

namespace Soma.Build
{
    public static class BuildProcess
    {
        private const string BuildFileRelativePathArg = "-buildSetupRelPath";

        public static void Build(BuildSetup buildSetup)
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
                    var targetGroup = BuildPipeline.GetBuildTargetGroup(target);

                    playerSettingsSnapshot.TakeSnapshot(targetGroup);

                    PlayerSettings.SetScriptingBackend(targetGroup, setup.scriptingBackend);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, $"{buildSetup.commonScriptingDefineSymbols},{setup.scriptingDefineSymbols}");

                    PlayerSettings.SetManagedStrippingLevel(targetGroup, setup.strippingLevel);

                    if (VRUtils.TargetGroupSupportsVirtualReality(targetGroup))
                    {
                        XRSettings.enabled = setup.supportsVR;
                    }
                    else
                    {
                        XRSettings.enabled = false;
                    }

                    if (target == BuildTarget.Android)
                    {
                        EditorUserBuildSettings.buildAppBundle = setup.androidAppBundle;
                        PlayerSettings.Android.targetArchitectures = setup.androidArchitecture;
                    }

                    if (target == BuildTarget.StandaloneOSX)
                    {
                        UserBuildSettings.architecture = setup.macOSArchitecture;
                    }

                    if (setup.rebuildAddressables)
                    {
                        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
                        AddressableAssetSettings.BuildPlayerContent();
                    }

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

        public static void Build(string buildSetupRelativePath)
        {
            var buildSetup = AssetDatabase.LoadAssetAtPath(buildSetupRelativePath, typeof(BuildSetup)) as BuildSetup;
            if (buildSetup != null)
            {
                Build(buildSetup);
            }
            else
            {
                Debug.LogError("Cannot find build setup in path: " + buildSetupRelativePath);
            }
        }

        public static void BuildWithArgs()
        {
            var buildFilePath = CLIUtils.GetCommandLineArg(BuildFileRelativePathArg);

            if (!string.IsNullOrEmpty(buildFilePath))
            {
                Build(buildFilePath);
            }
            else
            {
                Debug.LogError("Cannot find build setup path, make sure to specify using " + BuildFileRelativePathArg);
            }
        }
    }
}
