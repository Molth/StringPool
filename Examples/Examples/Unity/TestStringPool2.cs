#if UNITY_2021_3_OR_NEWER
using System.Buffers;
using Cysharp.Text;
using UnityEngine;

namespace Examples
{
    public sealed class TestStringPool2 : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                using (var builder1 = ZString.CreateStringBuilder())
                {
                    builder1.Append("Big Herta");
                    string a = StringPool.Shared.Rent(builder1.AsSpan());
                    Debug.Log(a);
                    StringPool.Shared.Return(a);
                }

                using (var builder2 = ZString.CreateStringBuilder())
                {
                    builder2.Append("Small Herta");
                    string b = StringPool.Shared.Rent(builder2.AsSpan());
                    Debug.Log(b);
                    StringPool.Shared.Return(b);
                }
            }
        }
    }
}
#endif