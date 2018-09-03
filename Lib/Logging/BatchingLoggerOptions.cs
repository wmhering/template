using System;
using System.Collections.Generic;
using System.Text;

namespace CuyahogaHHS.Logging
{
    public class BatchingLoggerOptions
    {
        private int? _MaximumBatchSize = 32;
        private TimeSpan _MaximumFlushTime = TimeSpan.FromSeconds(5);
        private int? _MaximumQueueSize = null;

        public bool IsEnabled { get; set; }

        public int? MaximumBatchSize
        {
            get { return _MaximumBatchSize; }
            set
            {
                if ((value ?? 1) <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"{nameof(MaximumBatchSize)} must be a positive number.");
                _MaximumBatchSize = value;
            }
        }

        public TimeSpan MaximumFlushTime
        {
            get { return _MaximumFlushTime; }
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"{nameof(MaximumFlushTime)} must be positive.");
                _MaximumFlushTime = value;
            }
        }

        public int? MaximumQueueSize
        {
            get { return _MaximumQueueSize; }
            set
            {
                if ((value ?? 1) <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"{nameof(MaximumQueueSize)} must be a positive number.");
                _MaximumQueueSize = value;
            }
        }
    }
}
