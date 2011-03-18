using System;
using Wintellect.Sterling.Exceptions;

namespace Wintellect.Sterling.ElevatedTrust
{
    public class SterlingElevatedTrustException : SterlingException 
    {
        public SterlingElevatedTrustException(Exception ex) : base(string.Format("An exception occurred accessing the file system with elevated trust: {0}", ex), ex)
        {
            
        }
    }
}