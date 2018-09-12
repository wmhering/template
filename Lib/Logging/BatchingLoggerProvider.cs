using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CuyahogaHHS.Logging
{
    /// <summary>
    /// Base class for <see cref="ILoggerProvider"/>s that write log entries in batches rather than individually.</summary>
    /// <remarks>
    /// <para>To write a logger that inherits from this class a developper must:</para>
    /// <list type="bullet">
    /// <item>Create an options class that inherits from <see cref="BatchingLoggerOptions"/> and optionally implements properties for any additional
    /// configuration options the new logger may require.</item>
    /// <item>Create a logger provider class that inherits from this class and impements
    /// <see cref="WriteLogEntriesAsync(IEnumerable{LogEntry}, CancellationToken)"/> </item>
    /// </list>
    /// </remarks>
    public abstract class BatchingLoggerProvider : ILoggerProvider
    {
        private readonly TimeSpan _MaximumWaitInterval;
        private readonly int _MaximumQueueSize;
        private readonly int _MaximumBatchSize;

        private CancellationTokenSource _CancellationTokenSource;
        private BlockingCollection<LogEntry> _LogEntryQueue;
        private readonly List<LogEntry> _CurrentBatch = new List<LogEntry>();
        private Task _OutputTask;

        /// <summary>
        /// Create a new <see cref="BatchingLoggerProvider"/>.</summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <remarks>
        /// <para>This class should be getting the default <see cref="LogLevel"/> and any filters from configuration.</para>
        /// </remarks>
        protected BatchingLoggerProvider(IOptions<BatchingLoggerOptions> options, IConfiguration configuration)
        {
            // Validate options one more time
            var loggerOptions = options.Value;
            if (loggerOptions.MaximumBatchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(loggerOptions.MaximumBatchSize),
                    $"{nameof(loggerOptions.MaximumBatchSize)} must be a positive number.");
            if (loggerOptions.MaximumQueueSize <= loggerOptions.MaximumBatchSize)
                throw new ArgumentOutOfRangeException(nameof(loggerOptions.MaximumQueueSize),
                    $"{nameof(loggerOptions.MaximumQueueSize)} must be larger than {nameof(loggerOptions.MaximumBatchSize)}.");
            if (loggerOptions.MaximumWaitInterval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(loggerOptions.MaximumWaitInterval),
                    $"{nameof(loggerOptions.MaximumWaitInterval)} must be longer than zero.");
            // Save settings
            _MaximumBatchSize = loggerOptions.MaximumBatchSize;
            _MaximumQueueSize = loggerOptions.MaximumQueueSize;
            _MaximumWaitInterval = loggerOptions.MaximumWaitInterval;
            // Start logging
            Start();
        }

        /// <summary>
        /// Write messages to the media the inheriting logger factory manages.</summary>
        /// <param name="logEntries">
        /// The log entries to be written.</param>
        /// <param name="token">
        /// A cancellation token that may be used to cancel logging.</param>
        /// <returns>
        /// A Task that can be waited upon.</returns>
        protected abstract Task WriteLogEntriesAsync(IEnumerable<LogEntry> logEntries, CancellationToken token);

        // Write remainding log entries to the queue during shutdown.
        private void ClearQueue()
        {
            var cts = new CancellationTokenSource();
            while (_LogEntryQueue.Count>0)
            {
                for (var i = _MaximumBatchSize; i > 0 && _LogEntryQueue.TryTake(out var logEntry); i--)
                    _CurrentBatch.Add(logEntry);
                try
                {
                     WriteLogEntriesAsync(_CurrentBatch, cts.Token);
                }
                catch(Exception ex)
                {
                    LogLoggerError(ex);
                }
            }
        }

        void LogLoggerError(Exception ex)
        { }

        // Process log entries in the queue until the logger is stopped.
        private async Task ProcessLogEntryQueue(object state)
        {
            while (!_CancellationTokenSource.IsCancellationRequested)
            {
                // Create a batch of log entries to be saved
                for (var i = _MaximumBatchSize; i > 0 && _LogEntryQueue.TryTake(out var message); i--)
                    _CurrentBatch.Add(message);
                // If there is a batch of records then save them.
                if (_CurrentBatch.Count > 0)
                {
                    try
                    {
                        await WriteLogEntriesAsync(_CurrentBatch, _CancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        LogLoggerError(ex);
                    }
                    _CurrentBatch.Clear();
                    // If there was a request to cancel then exit this loop.
                    if (_CancellationTokenSource.IsCancellationRequested)
                        continue;
                    // If there are enough log entries for another complete batch then process them.
                    if (_LogEntryQueue.Count >= _MaximumBatchSize)
                        continue;
                }
                // Otherwise wait one interval before 
                await IntervalAsync(_MaximumWaitInterval, _CancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// Wait an interval of time or until the process is canceled.</summary>
        /// <param name="interval">
        /// The amount of time to wait.</param>
        /// <param name="cancellationToken">
        /// The token used to cancel the wait period.</param>
        /// <returns>
        /// A Task that can be waited upon.</returns>
        protected virtual Task IntervalAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            return Task.Delay(interval, cancellationToken);
        }

        // Add a log entry to the queue.
        internal void AddMessage(LogEntry logEntry)
        {
            if(!_LogEntryQueue.IsAddingCompleted)
                try
                {
                    _LogEntryQueue.Add(logEntry, _CancellationTokenSource.Token);
                }
                catch
                { }
        }

        // Start processing the log entry queue
        private void Start()
        {
            _LogEntryQueue = new BlockingCollection<LogEntry>(new ConcurrentQueue<LogEntry>(), _MaximumQueueSize);
            _CancellationTokenSource = new CancellationTokenSource();
            _OutputTask = Task.Factory.StartNew<Task>(ProcessLogEntryQueue, null, TaskCreationOptions.LongRunning);
        }

        // Stop processing the log entry queue.
        private void Stop()
        {
            _CancellationTokenSource.Cancel();
            _LogEntryQueue.CompleteAdding();
            try
            {
                _OutputTask.Wait(_MaximumWaitInterval);
                ClearQueue();
            }
            catch (TaskCanceledException)
            { }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException)
            { }
        }

        #region ILoggerProvider interface
        /// <summary>
        /// Create a logger that uses this provider to write messages to media.</summary>
        /// <param name="categoryName">
        /// The category name to use to filter logg entries.</param>
        /// <returns>
        /// The new logger.</returns>
        ILogger ILoggerProvider.CreateLogger(string categoryName)
        {
            return new BatchingLogger(this, categoryName);
        }

        /// <summary>
        /// Stop processing log messages when the logger factory is disposed.</summary>
        public virtual void Dispose()
        {
            Stop();
        }
        #endregion
    }
}
