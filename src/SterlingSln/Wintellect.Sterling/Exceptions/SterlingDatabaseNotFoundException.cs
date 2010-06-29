using System;

namespace Wintellect.Sterling.Exceptions
{
    public class SterlingDatabaseNotFoundException : Exception
    {
        public SterlingDatabaseNotFoundException(string databaseName)
            : base(string.Format(Exceptions.SterlingDatabaseNotFoundException, databaseName))
        {
        }
    }
}