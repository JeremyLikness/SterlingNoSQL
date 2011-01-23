using System;

namespace Wintellect.Sterling.Database
{
    public abstract class BaseSterlingByteInterceptor : ISterlingByteInterceptor
    {
        virtual public byte[] Save(byte[] sourceStream)
        {
            throw new NotImplementedException();
        }

        virtual public byte[] Load(byte[] sourceStream)
        {
            throw new NotImplementedException();
        }
    }

}
