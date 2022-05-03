using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using UnityEditor;

#if UNITY_EDITOR_OSX
using UnityEditor.OSXStandalone;
#endif
namespace Soma.Build
{
    public enum SomaBuildTarget
    {
        NoTarget = BuildTarget.NoTarget,
        MacOS = BuildTarget.StandaloneOSX,
        Windows = BuildTarget.StandaloneWindows64,
        iOS = BuildTarget.iOS,
        Android = BuildTarget.Android
    }

    #if UNITY_EDITOR_OSX
    public enum SomaMacOSArchitecture
    {
        Intel64 = MacOSArchitecture.x64,
        AppleSilicon = MacOSArchitecture.ARM64,
        Universal = MacOSArchitecture.x64ARM64,
    }
    #endif
    
    [Serializable]
    public class BuildSetupEntry
    {
        public bool enabled = true;
        public string buildName = "";
        public SomaBuildTarget target = SomaBuildTarget.NoTarget;
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
        public bool strictMode;
        public bool detailedBuildReport;
        // Addressable
        public bool buildAddressables;
        public bool contentOnlyBuild;
        public string contentStateBinPathAddressable;
        public string profileNameAddressable = "";

        //MacOS
#if UNITY_EDITOR_OSX
        public SomaMacOSArchitecture macOSArchitecture;
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
        [NonSerialized] public bool _guiShowAddressableOptions = false;

        public static BuildSetupEntry Clone(BuildSetupEntry source)
        {
            return source.MemberwiseClone() as BuildSetupEntry;
        }
    }
}
