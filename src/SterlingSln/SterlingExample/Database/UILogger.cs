using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SterlingExample.Model;
using Wintellect.Sterling;

namespace SterlingExample.Database
{
    /// <summary>
    ///     Keeps a rolling list of messages
    /// </summary>
    public class UILogger
    {
        public UILogger()
        {
            Messages = DesignerProperties.IsInDesignTool
                           ? new ObservableCollection<LogEntry>(new[]
                                                                    {
                                                                        new LogEntry
                                                                            {
                                                                                ExceptionInfo = null,
                                                                                Message = "Message 1",
                                                                                Severity = SterlingLogLevel.Information
                                                                            },
                                                                        new LogEntry
                                                                            {
                                                                                ExceptionInfo = null,
                                                                                Message = "Message 2",
                                                                                Severity = SterlingLogLevel.Warning
                                                                            },
                                                                        new LogEntry
                                                                            {
                                                                                ExceptionInfo =
                                                                                    new Exception("Test exception"),
                                                                                Message = "Bad",
                                                                                Severity = SterlingLogLevel.Critical
                                                                            }
                                                                    })
                           : new ObservableCollection<LogEntry>();
        }
        
        public ObservableCollection<LogEntry> Messages { get; private set; }

        public void SetMessage(SterlingLogLevel level, string message, Exception ex)
        {
            var entry = new LogEntry {ExceptionInfo = ex, Message = message, Severity = level};           
            Messages.Add(entry);
        }
    }
}