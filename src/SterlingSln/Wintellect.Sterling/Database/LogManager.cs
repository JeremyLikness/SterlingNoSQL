﻿using System;
using System.Collections.Generic;
using System.Windows;
using Wintellect.Sterling.Exceptions;

namespace Wintellect.Sterling.Database
{
    /// <summary>
    ///     Manages the loggers
    /// </summary>
    internal class LogManager : ISterlingLock 
    {
        /// <summary>
        ///     The dictionary of loggers
        /// </summary>
        private readonly Dictionary<Guid,Action<SterlingLogLevel, string, Exception>> _loggers 
            = new Dictionary<Guid,Action<SterlingLogLevel, string, Exception>>();

        /// <summary>
        ///     Register a logger
        /// </summary>
        /// <param name="logger">The logger to register</param>
        /// <returns>A unique identifier</returns>
        public Guid RegisterLogger(Action<SterlingLogLevel, string, Exception> logger)
        {
            var identifier = Guid.NewGuid();

            lock(Lock)
            {
                _loggers.Add(identifier, logger);
            }

            return identifier;
        }

        /// <summary>
        ///     Removes a logger
        /// </summary>
        /// <param name="guid">The identifier for the logger</param>
        public void UnhookLogger(Guid guid)
        {
            if (!_loggers.ContainsKey(guid))
            {
                throw new SterlingLoggerNotFoundException(guid);
            }

            lock(Lock)
            {
                _loggers.Remove(guid);
            }
        }

        /// <summary>
        ///     Log an entry
        /// </summary>
        /// <param name="level">The level</param>
        /// <param name="message">The message</param>
        /// <param name="exception">The exception</param>
        public void Log(SterlingLogLevel level, string message, Exception exception)
        {
            var dispatcher = Deployment.Current.Dispatcher;
            
            lock(Lock)
            {
                foreach (var key in _loggers.Keys)
                {
                    var key1 = key;
                    Action action = () => _loggers[key1](level, message, exception);
                    if (dispatcher.CheckAccess())
                    {
                        action();
                    }
                    else
                    {
                        dispatcher.BeginInvoke(action);
                    }
                }
            }
        }

        private static readonly object _lock = new object();

        public object Lock
        {
            get { return _lock; }
        }
    }
}
