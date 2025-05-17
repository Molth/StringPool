using System.Buffers;
using Cysharp.Text;

namespace Examples
{
    internal sealed class Program
    {
        private static void Main()
        {
            Test2();
        }

        private static void Test1()
        {
            string a = StringPool.Shared.Rent("Big Herta".Length);
            var span = StringPool.AsSpan(a);
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
    }
}