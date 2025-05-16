using System.Buffers;
using NativeCollections;

namespace Examples
{
    internal sealed class Program
    {
        private static void Main()
        {
            string a = StringPool.Shared.Rent("Big Herta".Length);
            NativeString str = NativeString.Create(a, 0);
            str.Append("Big Herta");
            Console.WriteLine(a);
            StringPool.Shared.Return(a);

            string b = StringPool.Shared.Rent("Small Herta");
            Console.WriteLine(b);
            StringPool.Shared.Return(b);
        }
    }
}