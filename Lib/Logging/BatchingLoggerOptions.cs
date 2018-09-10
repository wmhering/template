using System;

using Microsoft.Extensions.Logging;

namespace CuyahogaHHS.Logging
{
    /// <summary>
    /// Configuration options for the <see cref="BatchingLoggerProvider"/> and the base class for all loggier providers that inherit from
    /// BatchingLoggerProvider.</summary>
    public class BatchingLoggerOptions
    {
        private int _MaximumBatchSize = 32;
        private int _MaximumQueueSize = 1024;
        private TimeSpan _MaximumWaitInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Gets/sets the maximum number of log entries that will be written to the output media.</summary>
        public int MaximumBatchSize
        {
            get { return _MaximumBatchSize; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"{nameof(MaximumBatchSize)} must be a positive number.");
                _MaximumBatchSize = value;
            }
        }

        /// <summary>
        /// Gets/sets the maximum number of log entries that can be queued before the logger will start dropping log entries.</summary>
        public int MaximumQueueSize
        {
            get { return _MaximumQueueSize; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"{nameof(MaximumQueueSize)} must be a positive number.");
                _MaximumQueueSize = value;
            }
        }

        /// <summary>
        ///  Gets/sets the maximum time log entries can wait in the queue before they are written to the output media.</summary>
        public TimeSpan MaximumWaitInterval
        {
            get { return _MaximumWaitInterval; }
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"{nameof(MaximumWaitInterval)} must be positive.");
                _MaximumWaitInterval = value;
            }
        }
    }
}
