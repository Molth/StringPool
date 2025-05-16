#if UNITY_2021_3_OR_NEWER
using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Cysharp.Text;
using TMPro;
using UnityEngine;

namespace Examples
{
    public sealed class TestStringPool3 : MonoBehaviour
    {
        public TMP_Text text;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                string a = StringPool.Shared.Rent(10000);
                using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();
                builder.Append("Herta");
                ReadOnlySpan<char> span = a.AsSpan();
                Span<char> span2 = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(span), span.Length);
                builder.TryCopyTo(span2, out _);
                text.text = a;
            }
        }
    }
}
#endif