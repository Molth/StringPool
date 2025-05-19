using System.Buffers;
using Cysharp.Text;
using UnityEngine;

namespace Examples
{
    public sealed class Example2 : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                using (var builder1 = ZString.CreateStringBuilder())
                {
                    builder1.Append("Big Herta");
                    var str1 = StringPool.Shared.Rent(builder1.AsSpan());
                    Debug.Log(str1);
                    StringPool.Shared.Return(str1);
                }

                using (var builder2 = ZString.CreateStringBuilder())
                {
                    builder2.Append("Small Herta");
                    var str2 = StringPool.Shared.Rent(builder2.AsSpan());
                    Debug.Log(str2);
                    StringPool.Shared.Return(str2);
                }
            }
        }
    }
}