using System;
using Wintellect.Sterling;

namespace SterlingExample.Model
{
    /// <summary>
    ///     A log entry 
    /// </summary>
    public class LogEntry
    {
        public LogEntry()
        {
            ID = Guid.NewGuid();
        }

        public Guid ID { get; private set; }
        
        /// <summary>
        ///     The severity of the log 
        /// </summary>
        public SterlingLogLevel Severity { get; set; }

        /// <summary>
        ///     The message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     Exception information
        /// </summary>
        public Exception ExceptionInfo { get; set; }

        public override bool Equals(object obj)
        {
            return obj is LogEntry && ((LogEntry) obj).ID.Equals(ID);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
