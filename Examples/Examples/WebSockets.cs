using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Text;

// ReSharper disable ALL

namespace Examples
{
    public static class WebSockets
    {
        public static void Test()
        {
            using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();

            builder.Append(Guid.NewGuid());
            ReadOnlySpan<char> str = builder.AsSpan();

            string str1 = SHA1ToBase64String(str);

            UnsafeValueString buffer = new UnsafeValueString();
            buffer.SetText(str);
            SHA1ToBase64String2(ref buffer);
            string? str2 = (string?)buffer;

            Console.WriteLine(str1 == str2);

            // do again

            builder.Clear();
            builder.Append(Guid.NewGuid());
            str = builder.AsSpan();

            str1 = SHA1ToBase64String(str);

            buffer.SetText(str);
            SHA1ToBase64String2(ref buffer);
            str2 = (string?)buffer;

            Console.WriteLine(str1 == str2);

            buffer.Dispose();
        }

        public static void Test2()
        {
            using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();

            builder.Append(Guid.NewGuid());
            ReadOnlySpan<char> str = builder.AsSpan();

            string str1 = SHA1ToBase64String(str);

            string str2 = SHA1ToBase64String3(str);

            Console.WriteLine(str1 == str2);

            // do again

            builder.Clear();
            builder.Append(Guid.NewGuid());
            str = builder.AsSpan();

            str1 = SHA1ToBase64String(str);

            str2 = SHA1ToBase64String3(str);

            Console.WriteLine(str1 == str2);
        }

        public static string SHA1ToBase64String(ReadOnlySpan<char> key)
        {
            int byteCount = Encoding.UTF8.GetByteCount(key);
            Span<byte> source = stackalloc byte[byteCount];
            _ = Encoding.UTF8.GetBytes(key, source);
            Span<byte> bytes = stackalloc byte[20];
            _ = SHA1.HashData(source, bytes);

            return Convert.ToBase64String(bytes);
        }

        public static void SHA1ToBase64String2(ref UnsafeValueString key)
        {
            int byteCount = Encoding.UTF8.GetByteCount((string?)key!);
            Span<byte> source = stackalloc byte[byteCount];
            _ = Encoding.UTF8.GetBytes((string?)key!, source);
            Span<byte> bytes = stackalloc byte[20];
            _ = SHA1.HashData(source, bytes);

            key.SetLength(28);
            Convert.TryToBase64Chars(bytes, StringPool.AsSpan(key!), out _);
        }

        [ThreadStatic] private static StrongReference<UnsafeString>? _key;

        public static string SHA1ToBase64String3(ReadOnlySpan<char> str)
        {
            UnsafeString key = (_key ??= new StrongReference<UnsafeString>(new UnsafeString())).Value!;
            key.SetText(str);

            int byteCount = Encoding.UTF8.GetByteCount((string?)key!);
            Span<byte> source = stackalloc byte[byteCount];
            _ = Encoding.UTF8.GetBytes((string?)key!, source);
            Span<byte> bytes = stackalloc byte[20];
            _ = SHA1.HashData(source, bytes);

            key.SetLength(28);
            Convert.TryToBase64Chars(bytes, StringPool.AsSpan(key!), out _);

            return (string?)key!;
        }
    }
}