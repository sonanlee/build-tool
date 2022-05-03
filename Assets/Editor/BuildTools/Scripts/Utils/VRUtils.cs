using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR;

namespace Soma.Build
{
    public static class VRUtils
    {
        public static string[] GetAvailableVRSdks(BuildTargetGroup targetGroup)
        {
            return XRSettings.supportedDevices;
        }

        public static string[] GetSelectedVRSdksFromFlags(BuildTargetGroup targetGroup, int flags)
        {
            var result = new List<string>();

            var vrSdks = GetAvailableVRSdks(targetGroup);
            for (var i = 0; i < vrSdks.Length; i++)
            {
                var layer = 1 << i;
                if ((flags & layer) != 0)
                {
                    result.Add(vrSdks[i]);
                }
            }

            return result.ToArray();
        }

        public static bool TargetGroupSupportsVirtualReality(BuildTargetGroup targetGroup)
        {
            var vrSdks = GetAvailableVRSdks(targetGroup);
            return vrSdks.Length > 0;
        }
    }
}
