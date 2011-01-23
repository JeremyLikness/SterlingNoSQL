using System;

namespace Wintellect.Sterling
{
    /// <summary>
    /// Byte Interceptor interface
    /// </summary>
    internal interface IByteInterceptor
    {
        byte[] Save(byte[] sourceStream);
        byte[] Load(byte[] sourceStream);
    }
}
