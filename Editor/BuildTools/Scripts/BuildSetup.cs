using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Soma.Build
{
    [Serializable]
    public class BuildSetup : ScriptableObject
    {
        public string exportDirectory = "";
        public bool abortBatchOnFailure;
        public string commonScriptingDefineSymbols = "";
        public List<BuildSetupEntry> entriesList;

        public static BuildSetup Create()
        {
            var asset = CreateInstance<BuildSetup>();

            AssetDatabase.CreateAsset(asset, BuildUtils.SetupsDirectory + "BuildSetup.asset");
            AssetDatabase.SaveAssets();
            return asset;
        }

        public void AddBuildSetupEntry()
        {
            var buildEntry = new BuildSetupEntry();

            var currentBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            buildEntry.buildName = Application.productName;
            buildEntry.target = (SomaBuildTarget)EditorUserBuildSettings.activeBuildTarget;
            buildEntry.customScenes = new List<string>();
            buildEntry.scriptingBackend = PlayerSettings.GetScriptingBackend(currentBuildTargetGroup);

            entriesList.Add(buildEntry);
        }

        public void DeleteBuildSetupEntry(BuildSetupEntry entry)
        {
            entriesList.Remove(entry);
        }

        public void DuplicateBuildSetupEntry(BuildSetupEntry entry)
        {
            var index = entriesList.IndexOf(entry);
            var buildEntry = BuildSetupEntry.Clone(entry);
            buildEntry.buildName = buildEntry.buildName + "_clone";
            entriesList.Insert(index + 1, buildEntry);
        }

        public void RearrangeBuildSetupEntry(BuildSetupEntry entry, bool up)
        {
            var oldIndex = entriesList.IndexOf(entry);
            var newIndex = up ? oldIndex - 1 : oldIndex + 1;

            if (newIndex >= 0 && newIndex < entriesList.Count)
            {
                var otherEntry = entriesList[newIndex];
                entriesList[newIndex] = entry;
                entriesList[oldIndex] = otherEntry;
            }
        }

        public bool IsReady()
        {
            var hasPath = !string.IsNullOrEmpty(exportDirectory);
            var hasEntries = entriesList.Any(e => e.enabled);

            return hasPath && hasEntries;
        }
    }
}
