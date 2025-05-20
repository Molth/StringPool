using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Text;

// ReSharper disable ALL

namespace Examples
{
    internal sealed class Program
    {
        private static void Main()
        {
            WebSockets.Test2();
        }

        private static void Test1()
        {
            string a = StringPool.Shared.Rent("Big Herta".Length);
            Span<char> span = StringPool.AsSpan(a);
            for (int i = 0; i < "Big Herta".Length; i++)
                span[i] = "Big Herta"[i];
            Console.WriteLine(a);
            StringPool.Shared.Return(a);

            string b = StringPool.Shared.Rent("Small Herta");
            Console.WriteLine(b);
            StringPool.Shared.Return(b);
        }

        private static void Test2()
        {
            using (Utf16ValueStringBuilder builder1 = ZString.CreateStringBuilder())
            {
                builder1.Append("Big Herta");
                string a = StringPool.Shared.Rent(builder1.AsSpan());
                Console.WriteLine(a);
                StringPool.Shared.Return(a);
            }

            using (Utf16ValueStringBuilder builder2 = ZString.CreateStringBuilder())
            {
                builder2.Append("Small Herta");
                string b = StringPool.Shared.Rent(builder2.AsSpan());
                Console.WriteLine(b);
                StringPool.Shared.Return(b);
            }
        }

        private static void Test3()
        {
            UnsafeString str1 = new UnsafeString();
            using UnsafeString str2 = new UnsafeString();

            using (Utf16ValueStringBuilder builder1 = ZString.CreateStringBuilder())
            {
                builder1.Append("Big Herta");
                str1.SetText(builder1.AsSpan());
                string? str = str1;
                if (str != null)
                {
                    Console.WriteLine("Dispose before: " + str.Length);
                    Console.WriteLine(str + ": " + str.Length);
                    byte[] hash1 = SHA256.HashData(Encoding.UTF8.GetBytes(str));
                    str1.Dispose();
                    Console.WriteLine("Disposed: " + str.Length);
                    Console.WriteLine();
                    byte[] hash2 = SHA256.HashData(Encoding.UTF8.GetBytes(str));
                    Console.WriteLine(hash1.AsSpan().SequenceEqual(hash2));
                    byte[] hash3 = SHA256.HashData(Encoding.UTF8.GetBytes("Big Herta"));
                    Console.WriteLine(hash1.AsSpan().SequenceEqual(hash3));
                    Console.WriteLine(hash2.AsSpan().SequenceEqual(hash3));
                    Console.WriteLine();
                }
            }

            using (Utf16ValueStringBuilder builder2 = ZString.CreateStringBuilder())
            {
                builder2.Append("Small Herta");
                str2.SetText(builder2.AsSpan());
                string? str = str2;
                if (str != null)
                    Console.WriteLine(str + ": " + str.Length);
            }
        }

        private static void Test4()
        {
            UnsafeValueString str1 = new UnsafeValueString();
            using UnsafeValueString str2 = new UnsafeValueString();

            using (Utf16ValueStringBuilder builder1 = ZString.CreateStringBuilder())
            {
                builder1.Append("Big Herta");
                str1.SetText(builder1.AsSpan());
                string? str = str1;
                if (str != null)
                {
                    Console.WriteLine("Dispose before: " + str.Length);
                    Console.WriteLine(str + ": " + str.Length);
                    byte[] hash1 = SHA256.HashData(Encoding.UTF8.GetBytes(str));
                    str1.Dispose();
                    Console.WriteLine("Disposed: " + str.Length);
                    Console.WriteLine();
                    byte[] hash2 = SHA256.HashData(Encoding.UTF8.GetBytes(str));
                    Console.WriteLine(hash1.AsSpan().SequenceEqual(hash2));
                    byte[] hash3 = SHA256.HashData(Encoding.UTF8.GetBytes("Big Herta"));
                    Console.WriteLine(hash1.AsSpan().SequenceEqual(hash3));
                    Console.WriteLine(hash2.AsSpan().SequenceEqual(hash3));
                    Console.WriteLine();
                }
            }

            using (Utf16ValueStringBuilder builder2 = ZString.CreateStringBuilder())
            {
                builder2.Append("Small Herta");
                str2.SetText(builder2.AsSpan());
                string? str = str2;
                if (str != null)
                    Console.WriteLine(str + ": " + str.Length);
            }
        }
    }
}