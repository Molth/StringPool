using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

#pragma warning disable CS8632

namespace Examples
{
    public static unsafe class MiniLog2
    {
        [ThreadStatic] private static UnsafeString? _buffer;

        public static void Log(ref NativeText fs)
        {
            var buffer = _buffer ??= new UnsafeString();

            buffer.SetLength(fs.Length * 2);

            int length;
            fixed (char* c = &MemoryMarshal.GetReference(((string?)buffer).AsSpan()))
            {
                Unicode.Utf8ToUtf16(fs.GetUnsafePtr(), fs.Length, c, out length, fs.Length * 2);
            }

            buffer.SetLength(length);
            Debug.Log(buffer);
        }
    }

    public class Example8 : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                var text = new NativeText(Allocator.Temp);

                text.Append("Big Herta");

                MiniLog2.Log(ref text);
                text.Dispose();
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                var text = new NativeText(Allocator.Temp);

                text.Append("Small Herta");

                MiniLog2.Log(ref text);
                text.Dispose();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                var text = new NativeText(Allocator.Temp);

                text.Append("高望しすぎだよ。\r\n");
                text.Append("いつも他人をバカみたいに思ってるけど…\r\n");
                text.Append("いつも上から目線だよね\r\n");
                text.Append("お前、全部パクってんじゃん！　偉そうにすんなよ\r\n");
                text.Append("まあ、謙虚にいこうぜ\r\n");
                text.Append("別に難しいことじゃないし\r\n");
                text.Append("ワロタ\r\n");
                text.Append("フリーランス最強！\r\n");
                text.Append("あれは商業機密だから…\r\n");
                text.Append("商業機密レベル作れるんなら、なんで面接受けないんだよ！\r\n");
                text.Append("マジ怖い\r\n");
                text.Append("ケンカ売ってんの！？\r\n");

                MiniLog2.Log(ref text);
                text.Dispose();
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                var text = new NativeText(Allocator.Temp);

                text.Append("So this is actually pretty useless, huh?\r\n");
                text.Append("I'm just saying that string pool of yours is useless.\r\n");

                MiniLog2.Log(ref text);
                text.Dispose();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                var text = new NativeText(Allocator.Temp);

                text.Append("I wish I were Herta, \r\n");
                text.Append("but I live a lifetime of living in obscurity, struggling to make ends meet, and enduring humiliation.\r\n");

                MiniLog2.Log(ref text);
                text.Dispose();
            }
        }
    }
}