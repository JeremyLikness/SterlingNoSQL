﻿using System;
using Wintellect.Sterling.Database;

namespace Wintellect.Sterling
{
    /// <summary>
    ///     Wrapper for the sterling database engine
    /// </summary>
    public class SterlingEngine : IDisposable
    {
        /// <summary>
        ///     The database engine
        /// </summary>
        public ISterlingDatabase SterlingDatabase { get; private set; }

        /// <summary>
        ///     True if it's been activated
        /// </summary>
        private bool _activated; 

        /// <summary>
        ///     Constructor takes in the database 
        /// </summary>
        public SterlingEngine()
        {
            SterlingDatabase = SterlingFactory.GetDatabaseEngine();            
        }

        public void Activate()
        {
            ((SterlingDatabase)SterlingDatabase).Activate();
            _activated = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_activated)
            {                
                ((SterlingDatabase)SterlingDatabase).Deactivate();
            }
        }
    }
}
