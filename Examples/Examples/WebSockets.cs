using System.Buffers;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable ALL

namespace Examples
{
    public static class WebSockets
    {
        public static void Test()
        {
            string str = Guid.NewGuid().ToString();

            string str1 = SHA1ToBase64String(str);

            UnsafeValueString buffer = new UnsafeValueString();
            buffer.SetText(str);
            SHA1ToBase64String2(ref buffer);
            string? str2 = (string?)buffer;

            Console.WriteLine(str1 == str2);

            // do again

            str = Guid.NewGuid().ToString();

            str1 = SHA1ToBase64String(str);

            buffer.SetText(str);
            SHA1ToBase64String2(ref buffer);
            str2 = (string?)buffer;

            Console.WriteLine(str1 == str2);

            buffer.Dispose();
        }

        public static void Test2()
        {
            string str = Guid.NewGuid().ToString();

            string str1 = SHA1ToBase64String(str);

            string str2 = SHA1ToBase64String3(str);

            Console.WriteLine(str1 == str2);

            // do again

            str = Guid.NewGuid().ToString();

            str1 = SHA1ToBase64String(str);

            str2 = SHA1ToBase64String3(str);

            Console.WriteLine(str1 == str2);
        }

        public static string SHA1ToBase64String(string key)
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

        [ThreadStatic] private static UnsafeString? _key;

        public static string SHA1ToBase64String3(string str)
        {
            UnsafeString key = _key ??= new UnsafeString();
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