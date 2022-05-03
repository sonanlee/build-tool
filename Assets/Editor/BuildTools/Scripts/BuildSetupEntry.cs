using System;
using System.Collections.Generic;
using UnityEditor;

#if UNITY_EDITOR_OSX
using UnityEditor.OSXStandalone;
#endif
namespace Soma.Build
{
    [Serializable]
    public class BuildSetupEntry
    {
        public bool enabled = true;
        public string buildName = "";
        public BuildTarget target = BuildTarget.NoTarget;
        public bool debugBuild;
        public string scriptingDefineSymbols = "";
        public bool useDefaultBuildScenes = true;
        public List<string> customScenes;

        //VR
        public bool supportsVR;
        public int vrSdkFlags;

        // Advanced Options
        public ManagedStrippingLevel strippingLevel;
        public ScriptingImplementation scriptingBackend = ScriptingImplementation.IL2CPP;
        public string assetBundleManifestPath = "";
        public bool strictMode;
        public bool detailedBuildReport;
        public bool rebuildAddressables;

        //MacOS
#if UNITY_EDITOR_OSX
        public MacOSArchitecture macOSArchitecture;
#endif
        //iOS
        public bool iosSymlinkLibraries;

        //Android
        public bool androidAppBundle;
        public AndroidArchitecture androidArchitecture;

        [NonSerialized] public bool _guiShowAdvancedOptions = false;
        [NonSerialized] public bool _guiShowCustomScenes = false;

        // GUI status
        [NonSerialized] public bool _guiShowOptions = true;
        [NonSerialized] public bool _guiShowVROptions = false;

        public static BuildSetupEntry Clone(BuildSetupEntry source)
        {
            return source.MemberwiseClone() as BuildSetupEntry;
        }
    }
}
