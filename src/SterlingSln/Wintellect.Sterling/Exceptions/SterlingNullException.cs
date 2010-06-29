using System;

namespace Wintellect.Sterling.Exceptions
{
    public class SterlingNullException : Exception 
    {
        public SterlingNullException(string property, Type type) : base(string.Format(Exceptions.SterlingNullException, property, type.FullName))
        {
            
        }
    }
}
