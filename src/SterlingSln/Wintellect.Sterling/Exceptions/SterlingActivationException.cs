using System;

namespace Wintellect.Sterling.Exceptions
{
    public class SterlingActivationException : Exception 
    {
        public SterlingActivationException(string operation) : base(string.Format(Exceptions.SterlingActivationException, operation))
        {
            
        }
    }
}
