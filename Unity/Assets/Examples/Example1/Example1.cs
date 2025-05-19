using System.Buffers;
using UnityEngine;

namespace Examples
{
    public sealed class Example1 : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Span<char>
                var str1 = StringPool.Shared.Rent("Big Herta".Length);
                {
                    var span = StringPool.AsSpan(str1);
                    for (var i = 0; i < "Big Herta".Length; i++)
                        span[i] = "Big Herta"[i];

                    Debug.Log(str1);

                    StringPool.Shared.Return(str1);
                }

                // ReadOnlySpan<char>
                var str2 = StringPool.Shared.Rent("Small Herta");
                {
                    Debug.Log(str2);

                    StringPool.Shared.Return(str2);
                }

                // same
                Debug.Log(ReferenceEquals(str1, str2));
            }
        }
    }
}