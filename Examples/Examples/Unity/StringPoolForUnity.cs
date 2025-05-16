#if UNITY_2021_3_OR_NEWER
using System.Buffers;
using UnityEngine;
using UnityEngine.Profiling;

namespace Examples
{
    public static class StringPoolForUnity
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            StringPool.Custom(GetMemoryPressure);
            return;

            static double GetMemoryPressure() => Profiler.GetTotalAllocatedMemoryLong() / (double)((long)SystemInfo.systemMemorySize * 1024 * 1024);
        }
    }
}
#endif