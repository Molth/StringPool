#if UNITY_2021_3_OR_NEWER
using System;
using System.Buffers;
using Cysharp.Text;
using UnityEngine;

namespace Examples
{
    public sealed class TestStringPoolGC : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                ExecuteMethod("Method_A", () =>
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        using (var builder1 = ZString.CreateStringBuilder())
                        {
                            builder1.Append("Big Herta");
                            string text1 = StringPool.Shared.Rent(builder1.AsSpan());
                            Debug.Log(text1);
                            StringPool.Shared.Return(text1);
                        }

                        using (var builder2 = ZString.CreateStringBuilder())
                        {
                            builder2.Append("Small Herta");
                            string text2 = StringPool.Shared.Rent(builder2.AsSpan());
                            Debug.Log(text2);
                            StringPool.Shared.Return(text2);
                        }
                    }
                });
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                ExecuteMethod("Method_B", () =>
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        using (var builder1 = ZString.CreateStringBuilder())
                        {
                            builder1.Append("Big Herta");
                            Debug.Log(builder1);
                        }

                        using (var builder2 = ZString.CreateStringBuilder())
                        {
                            builder2.Append("Small Herta");
                            Debug.Log(builder2);
                        }
                    }
                });
            }
        }

        private void ExecuteMethod(string methodName, Action action)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            long startMemory = GC.GetTotalMemory(true);

            action();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            long endMemory = GC.GetTotalMemory(false);

            Debug.Log($"[{methodName}] GC Analysis:\r\n" +
                      $"- Memory Delta: {((endMemory - startMemory) / 1024.0):F2} KB\r\n" +
                      $"- Total Memory: {endMemory / 1024.0:F2} KB");
        }
    }
}
#endif