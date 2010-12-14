﻿using System;

namespace Wintellect.Sterling.Exceptions
{
    public class SterlingLoggerNotFoundException : SterlingException 
    {
        public SterlingLoggerNotFoundException(Guid guid) : base(string.Format(Exceptions.SterlingLoggerNotFoundException, guid))
        {
            
        }
    }
}
