using System;

namespace Wintellect.Sterling.Exceptions
{
    public class SterlingDuplicateDatabaseException : Exception
    {
        public SterlingDuplicateDatabaseException(ISterlingDatabaseInstance instance) : base(
            string.Format(Exceptions.SterlingDuplicateDatabaseException, instance.GetType().FullName))
        {
        }

        public SterlingDuplicateDatabaseException(Type type)
            : base(
                string.Format(Exceptions.SterlingDuplicateDatabaseException, type.FullName))
        {
        }
    }
}