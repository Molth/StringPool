using System;
using System.Buffers;
using Cysharp.Text;
using UnityEngine;

#pragma warning disable CS8632

namespace Examples
{
    public static class MiniLog
    {
        [ThreadStatic] private static UnsafeString? _buffer;

        public static void Log(ReadOnlySpan<char> message)
        {
            var buffer = _buffer ??= new UnsafeString();
            buffer.SetText(message);
            Debug.Log(buffer);
        }
    }

    public class Example6 : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                using (var builder = ZString.CreateStringBuilder())
                {
                    builder.Append("Big Herta");
                    MiniLog.Log(builder.AsSpan());
                }
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                using (var builder = ZString.CreateStringBuilder())
                {
                    builder.Append("Small Herta");
                    MiniLog.Log(builder.AsSpan());
                }
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                using (var builder = ZString.CreateStringBuilder())
                {
                    builder.AppendLine("高望しすぎだよ。");
                    builder.AppendLine("いつも他人をバカみたいに思ってるけど…");
                    builder.AppendLine("いつも上から目線だよね");
                    builder.AppendLine("お前、全部パクってんじゃん！　偉そうにすんなよ");
                    builder.AppendLine("まあ、謙虚にいこうぜ");
                    builder.AppendLine("別に難しいことじゃないし");
                    builder.AppendLine("ワロタ");
                    builder.AppendLine("フリーランス最強！");
                    builder.AppendLine("あれは商業機密だから…");
                    builder.AppendLine("商業機密レベル作れるんなら、なんで面接受けないんだよ！");
                    builder.AppendLine("マジ怖い");
                    builder.AppendLine("ケンカ売ってんの！？");

                    MiniLog.Log(builder.AsSpan());
                }
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                using (var builder = ZString.CreateStringBuilder())
                {
                    builder.AppendLine("So this is actually pretty useless, huh?");
                    builder.AppendLine("I'm just saying that string pool of yours is useless.");

                    MiniLog.Log(builder.AsSpan());
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                using (var builder = ZString.CreateStringBuilder())
                {
                    builder.AppendLine("I wish I were Herta, ");
                    builder.AppendLine("but I live a lifetime of living in obscurity, struggling to make ends meet, and enduring humiliation.");
                    MiniLog.Log(builder.AsSpan());
                }
            }
        }
    }
}