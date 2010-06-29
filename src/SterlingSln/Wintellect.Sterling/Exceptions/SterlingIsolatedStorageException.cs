using System;

namespace Wintellect.Sterling.Exceptions
{
    public class SterlingIsolatedStorageException : Exception
    {
        public SterlingIsolatedStorageException(Exception ex) : base(string.Format(Exceptions.SterlingIsolatedStorageException,ex.Message), ex)
        {
            
        }
    }
}
