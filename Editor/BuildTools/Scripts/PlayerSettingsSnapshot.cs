using UnityEditor;
using UnityEngine.XR;

namespace Soma.Build
{
    public class PlayerSettingsSnapshot
    {
        public bool _androidAppBundleEnabled;

        private BuildTargetGroup _buildTargetGroup;
        private ScriptingImplementation _scriptingBackend;
        private string _scriptingDefineSymbols;
        private ManagedStrippingLevel _strippingLevel;
        
        private bool _vrSupported;
        private string _buildNumber;
        private int _bundleVersionCode;
        private string _bundleVersion;
        private string _productName;

        public void TakeSnapshot(BuildTargetGroup targetGroup)
        {
            _buildTargetGroup = targetGroup;

            _scriptingBackend = PlayerSettings.GetScriptingBackend(targetGroup);
            _scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            _strippingLevel = PlayerSettings.GetManagedStrippingLevel(targetGroup);

            _vrSupported = XRSettings.enabled;
            _androidAppBundleEnabled = EditorUserBuildSettings.buildAppBundle;
            
            _bundleVersionCode  = PlayerSettings.Android.bundleVersionCode;
            _buildNumber = PlayerSettings.iOS.buildNumber;
            _bundleVersion = PlayerSettings.bundleVersion;
            _productName = PlayerSettings.productName;
        }

        public void ApplySnapshot()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTargetGroup, _scriptingDefineSymbols);
            PlayerSettings.SetScriptingBackend(_buildTargetGroup, _scriptingBackend);
            PlayerSettings.SetManagedStrippingLevel(_buildTargetGroup, _strippingLevel);
            EditorUserBuildSettings.buildAppBundle = _androidAppBundleEnabled;
            
            XRSettings.enabled = _vrSupported;
            
            
            PlayerSettings.Android.bundleVersionCode = _bundleVersionCode;
            PlayerSettings.iOS.buildNumber = _buildNumber;
            PlayerSettings.bundleVersion = _bundleVersion;
            PlayerSettings.productName = _productName;
        }
    }
}
