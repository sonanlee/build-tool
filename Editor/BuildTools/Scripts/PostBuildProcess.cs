#if UNITY_IOS
using System.Diagnostics;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace Soma.Build
{
    public class PostBuildProcess : IPostprocessBuildWithReport // Will execute after XCode project is built
    {
        public int callbackOrder { get { return 0; } }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.iOS) // Check if the build is for iOS 
            {
                var outPath = report.summary.outputPath;
                var plistPath =  outPath+ "/Info.plist";
                var plist = new PlistDocument(); // Read Info.plist file into memory
                plist.ReadFromString(File.ReadAllText(plistPath));

                var rootDict = plist.root;
                rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

                File.WriteAllText(plistPath, plist.WriteToString()); // Override Info.plist

                // 만약에 In app 결제나 다른 프로세스가 필요할경우 활용
                // if (report.summary.platform == BuildTarget.iOS)
                // {
                //     var projectPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
                //     var project = new PBXProject();
                //     project.ReadFromFile(projectPath);
                //     var targetGuid = project.GetUnityMainTargetGuid();
                //     
                //     var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", null, targetGuid);
                //     manager.AddKeychainSharing(new []{"6Z4WF33SY3.com.somadevelopmentco.soma.dev", "6Z4WF33SY3.com.somadevelopmentco.soma"});
                //     manager.WriteToFile();     
                // }
            }
        }
    }
}
#endif
