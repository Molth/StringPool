using System.Buffers;
using Cysharp.Text;
using TMPro;
using UnityEngine;

namespace Examples
{
    public class Example5 : MonoBehaviour
    {
        public TMP_Text text;
        private UnsafeString _str;

        private void Start()
        {
            _str = new UnsafeString();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                using (var builder = ZString.CreateStringBuilder())
                {
                    builder.Append("Big Herta");
                    _str.SetText(builder.AsSpan());

                    // TODO: must refresh reference
                    text.text = null;

                    text.text = _str;
                    Debug.Log(text.text.Length);
                }
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                using (var builder = ZString.CreateStringBuilder())
                {
                    builder.Append("Small Herta");
                    _str.SetText(builder.AsSpan());

                    // TODO: must refresh reference
                    text.text = null;

                    text.text = _str;
                    Debug.Log(text.text.Length);
                }
            }
        }

        private void OnDestroy()
        {
            _str.Dispose();
        }
    }
}