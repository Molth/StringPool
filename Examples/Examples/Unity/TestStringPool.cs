#if UNITY_2021_3_OR_NEWER
using System.Buffers;
using UnityEngine;

namespace Examples
{
    public sealed class TestStringPool : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                string a = StringPool.Shared.Rent("Big Herta".Length);
                var span = StringPool.AsSpan(a);
                for (int i = 0; i < "Big Herta".Length; i++)
                    span[i] = "Big Herta"[i];
                Debug.Log(a);

                StringPool.Shared.Return(a);

                string b = StringPool.Shared.Rent("Small Herta");
                Debug.Log(b);

                StringPool.Shared.Return(b);

                Debug.Log(ReferenceEquals(a, b));
            }
        }
    }
}
#endif