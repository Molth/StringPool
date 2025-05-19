using UnityEngine;
using UnityEngine.Profiling;

namespace System.Buffers.Unity
{
    internal static class StringPoolForUnity
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