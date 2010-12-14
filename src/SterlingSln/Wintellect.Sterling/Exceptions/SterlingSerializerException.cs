using System;
using Wintellect.Sterling.Serialization;

namespace Wintellect.Sterling.Exceptions
{
    public class SterlingSerializerException : SterlingException 
    {
        public SterlingSerializerException(ISterlingSerializer serializer, Type targetType) : 
            base(string.Format(Exceptions.SterlingSerializerException, serializer.GetType().FullName, targetType.FullName))
        {
            
        }
    }
}
