namespace Examples
{
    public class StrongReference<T> : IDisposable where T : class, IDisposable
    {
        public T? Value;

        public StrongReference(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Value = value;
        }

        ~StrongReference() => Dispose();

        public void Dispose()
        {
            T? value = Interlocked.Exchange(ref Value, null);
            if (value == null)
                return;

            GC.SuppressFinalize(this);
            value.Dispose();
        }
    }
}