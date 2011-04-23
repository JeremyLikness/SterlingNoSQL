using System;
using Wintellect.Sterling.Exceptions;

namespace Wintellect.Sterling.Server.FileSystem
{
    public class SterlingFileSystemException : SterlingException 
    {
        public SterlingFileSystemException(Exception ex) : base(string.Format("An exception occurred accessing the file system: {0}", ex), ex)
        {
            
        }
    }
}