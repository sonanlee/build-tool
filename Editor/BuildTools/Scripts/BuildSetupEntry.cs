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

    public enum SomaAndroidArchitecture
    {
        Android = (int) (AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7),
        ChromeOS = (int) (AndroidArchitecture.X86 | AndroidArchitecture.X86_64)
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
        public string productName = "";
        public SomaBuildTarget target = SomaBuildTarget.NoTarget;
        public bool developmentBuild;
        public string scriptingDefineSymbols = "";
        public bool useDefaultBuildScenes = true;
        public List<string> customScenes;

        //VR
        public bool supportsVR;
        public int vrSdkFlags;

        // Advanced Options
        public bool buildClient = true;
        public ManagedStrippingLevel strippingLevel;
        public ScriptingImplementation scriptingBackend = ScriptingImplementation.IL2CPP;
        public bool strictMode;
        
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
        public SomaAndroidArchitecture androidArchitecture;

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
