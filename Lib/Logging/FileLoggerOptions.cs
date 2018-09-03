using System;

namespace CuyahogaHHS.Logging
{
    public class FileLoggerOptions : BatchingLoggerOptions
    {
        private string _FileNameRoot;
        private string _LogDirectory;
        private int _MaximumFileCount;
        private int _MaximumFileSize;

        public string FileNameRoot
        {
            get { return _FileNameRoot; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException(nameof(FileNameRoot));
                _FileNameRoot = value;
            }
        }

        public string LogDirectory
        {
            get { return _LogDirectory; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException(nameof(LogDirectory));
                _LogDirectory = value;
            }
        }

        public int MaximumFileCount
        {
            get { return _MaximumFileCount; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(MaximumFileCount)} must be positive.");
                _MaximumFileCount = value;
            }
        }

        public int MaximumFileSize
        {
            get { return _MaximumFileSize; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(MaximumFileSize)} must be positive.");
                _MaximumFileSize = value;
            }
        }
    }
}
