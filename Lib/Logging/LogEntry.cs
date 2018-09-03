using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CuyahogaHHS.Logging
{
    public class LogEntry
    {
        public LogEntry(DateTimeOffset timestamp, string category, LogLevel logLevel, EventId eventId, Exception exception, string message)
        {
            Timestamp = timestamp;
            Category = category;
            LogLevel = logLevel;
            EventId = eventId;
            Exception = exception;
            Message = message;
        }

        public string Category { get; }

        public EventId EventId { get; }

        public Exception Exception { get; }

        public LogLevel LogLevel { get; }

        public string Message { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
