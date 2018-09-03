using System;

using Microsoft.Extensions.Logging;

namespace CuyahogaHHS.Logging
{
    internal class BatchingLogger : ILogger
    {
        private string _Category;
        private BatchingLoggerProvider _Provider;


        internal BatchingLogger(BatchingLoggerProvider provider, string category)
        {
            _Provider = provider;
            _Category = category;
        }

        #region ILogger interface
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (logLevel != LogLevel.None);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _Provider.AddMessage(new LogEntry(DateTimeOffset.Now, _Category, logLevel, eventId, exception, formatter(state, exception)));
        }
        #endregion
    }
}
