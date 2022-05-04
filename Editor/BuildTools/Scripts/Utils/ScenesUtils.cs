using System.Linq;
using UnityEditor;


namespace Soma.Build
{
    public static class ScenesUtils
    {
        public static string[] GetDefaultScenesAsArray()
        {
            var scenes = EditorBuildSettings.scenes;

            return scenes.Where(x => x.enabled).Select(x => x.path).ToArray();
        }
    }
}
