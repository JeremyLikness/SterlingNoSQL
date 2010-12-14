using System;

namespace Wintellect.Sterling.Exceptions
{
    public class SterlingTableNotFoundException : SterlingException
    {
        public SterlingTableNotFoundException(Type tableType, string databaseName)
            : base(string.Format(Exceptions.SterlingTableNotFoundException, tableType.FullName, databaseName))
        {
        }
    }
}