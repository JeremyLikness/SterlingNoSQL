using System;

namespace Wintellect.Sterling
{
    /// <summary>
    /// Byte Interceptor interface
    /// </summary>
    internal interface ISterlingByteInterceptor
    {
        byte[] Save(byte[] sourceStream);
        byte[] Load(byte[] sourceStream);
    }
}
