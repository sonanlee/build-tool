using UnityEditor;

namespace Soma.Build
{
    public static class ScenesUtils
    {
        public static string[] GetDefaultScenesAsArray()
        {
            var scenes = EditorBuildSettings.scenes;

            var result = new string[scenes.Length];

            for (var i = 0; i < scenes.Length; i++)
            {
                result[i] = scenes[i].path;
            }

            return result;
        }
    }
}
