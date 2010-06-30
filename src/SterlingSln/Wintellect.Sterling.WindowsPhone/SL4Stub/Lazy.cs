namespace System
{
    /// <summary>
    ///     Used to stub in what is missing from the Windows Phone 7 runtime
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Lazy<T>
    {
        private readonly object _sync = new object();
        private readonly Func<T> _getter;
        private T _instance;

        public bool IsValueCreated { get; private set; }

        public Lazy(Func<T> getter)
        {
            _getter = getter;
        }

        public T Value
        {
            get
            {
                if (!IsValueCreated)
                {
                    lock(_sync)
                    {
                        if (!IsValueCreated)
                        {
                            _instance = _getter();
                            IsValueCreated = true;
                        }
                    }
                }
                return _instance;
            }
        }
    }  
}
