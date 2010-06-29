﻿using System;

namespace Wintellect.Sterling.Exceptions
{
    public class SterlingIndexNotFoundException : Exception 
    {
        public SterlingIndexNotFoundException(string indexName, Type type) : 
            base(string.Format(Exceptions.SterlingIndexNotFoundException, indexName, type.FullName))
        {
            
        }
    }
}
