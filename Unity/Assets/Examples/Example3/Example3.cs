using System.Buffers;
using Cysharp.Text;
using TMPro;
using UnityEngine;

namespace Examples
{
    public sealed class Example3 : MonoBehaviour
    {
        public TMP_Text text;
        private string _str;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_str != null)
                    StringPool.Shared.Return(_str);

                _str = StringPool.Shared.Rent(10000);

                using var builder = ZString.CreateStringBuilder();
                builder.Append("Herta");

                var span = StringPool.AsSpan(_str);
                builder.TryCopyTo(span, out _);

                text.text = _str;
            }
        }
    }
}