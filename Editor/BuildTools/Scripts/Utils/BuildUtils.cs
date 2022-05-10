using System.IO;
using UnityEditor;

namespace Soma.Build
{
    public static class BuildUtils
    {
        public const string SetupsDirectory = "Assets/Plugins/build-tool/Editor/BuildTools/";
        private const string WindowsExtension = ".exe";
        private const string AndroidExtension = ".apk";
        private const string AndroidStoreExtension = ".aab";

        public static BuildPlayerOptions GetBuildPlayerOptionsFromBuildSetupEntry(BuildSetupEntry setupEntry, string rootDirPath, string[] defaultScenes)
        {
            var buildPlayerOptions = new BuildPlayerOptions {target = (BuildTarget)setupEntry.target};

            if (setupEntry.useDefaultBuildScenes)
            {
                buildPlayerOptions.scenes = defaultScenes;
            }
            else
            {
                buildPlayerOptions.scenes = setupEntry.customScenes.ToArray();
            }

            var pathName = Path.Combine(rootDirPath, setupEntry.buildName, setupEntry.productName);
            if (setupEntry.target == SomaBuildTarget.Windows)
            {
                if (!pathName.Contains(WindowsExtension))
                {
                    pathName += WindowsExtension;
                }
            }

            if (setupEntry.target == SomaBuildTarget.Android)
            {
                var ext = AndroidExtension;
                if (setupEntry.androidAppBundle)
                {
                    ext = AndroidStoreExtension;
                }
                if (!pathName.Contains(ext))
                {
                    pathName += ext;
                }
            }

            buildPlayerOptions.locationPathName = pathName;
            

            var buildOptions = BuildOptions.None;
            if (setupEntry.developmentBuild)
            {
                buildOptions |= BuildOptions.Development;
            }

            if (setupEntry.strictMode)
            {
                buildOptions |= BuildOptions.StrictMode;
            }

            if (setupEntry.target == SomaBuildTarget.iOS)
            {
                if (setupEntry.iosSymlinkLibraries)
                {
                    buildOptions |= BuildOptions.SymlinkSources;
                }
            }

            buildPlayerOptions.options = buildOptions;

            return buildPlayerOptions;
        }
    }
}
