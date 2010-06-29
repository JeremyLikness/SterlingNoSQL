using System;

namespace Wintellect.Sterling.Exceptions
{
    public class SterlingLoggerNotFoundException : Exception 
    {
        public SterlingLoggerNotFoundException(Guid guid) : base(string.Format(Exceptions.SterlingLoggerNotFoundException, guid))
        {
            
        }
    }
}
