using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CuyahogaHHS.Logging
{
    public abstract class BatchingLoggerProvider : ILoggerProvider
    {
        private readonly List<LogEntry> _CurrentBatch = new List<LogEntry>();
        private readonly TimeSpan _Interval;
        private readonly int? _MaximumQueueSize;
        private readonly int? _MaximumBatchSize;

        private CancellationTokenSource _CancellationTokenSource;
        private BlockingCollection<LogEntry> _MessageQueue;
        private Task _OutputTask;

        protected BatchingLoggerProvider(IOptions<BatchingLoggerOptions> options)
        {
            var loggerOptions = options.Value;
            _Interval = loggerOptions.MaximumFlushTime;
            _MaximumBatchSize = loggerOptions.MaximumBatchSize;
            _MaximumQueueSize = loggerOptions.MaximumQueueSize;
        }

        protected abstract Task WriteMessagesAsync(IEnumerable<LogEntry> messages, CancellationToken token);

        private async Task ProcessLogQueue(object state)
        {
            while (!_CancellationTokenSource.IsCancellationRequested)
            {
                var limit = _MaximumBatchSize ?? int.MaxValue;
                while (limit > 0 && _MessageQueue.TryTake(out var message))
                {
                    _CurrentBatch.Add(message);
                    limit--;
                }
                if (_CurrentBatch.Count > 0)
                {
                    try
                    {
                        await WriteMessagesAsync(_CurrentBatch, _CancellationTokenSource.Token);
                    }
                    catch
                    { }
                    _CurrentBatch.Clear();

                }
                await IntervalAsync(_Interval, _CancellationTokenSource.Token);
            }
        }

        protected virtual Task IntervalAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            return Task.Delay(interval, cancellationToken);
        }

        internal void AddMessage(LogEntry logEntry)
        {
            if(!_MessageQueue.IsAddingCompleted)
                try
                {
                    _MessageQueue.Add(logEntry, _CancellationTokenSource.Token);
                }
                catch
                { }
        }

        private void Start()
        {
            _MessageQueue = !_MaximumQueueSize.HasValue
                ? new BlockingCollection<LogEntry>(new ConcurrentQueue<LogEntry>())
                : new BlockingCollection<LogEntry>(new ConcurrentQueue<LogEntry>(), _MaximumQueueSize.Value);
            _CancellationTokenSource = new CancellationTokenSource();
            _OutputTask = Task.Factory.StartNew<Task>(ProcessLogQueue, null, TaskCreationOptions.LongRunning);
        }

        private void Stop()
        {
            _CancellationTokenSource.Cancel();
            _MessageQueue.CompleteAdding();
            try
            {
                _OutputTask.Wait(_Interval);
            }
            catch (TaskCanceledException)
            { }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException)
            { }
        }

        #region ILoggerProvider interface
        ILogger ILoggerProvider.CreateLogger(string categoryName)
        {
            return new BatchingLogger(this, categoryName);
        }

        public void Dispose()
        {
            Stop();
        }
        #endregion
    }
}
