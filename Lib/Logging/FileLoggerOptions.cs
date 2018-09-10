using System;

namespace CuyahogaHHS.Logging
{
    /// <summary>
    /// Configuration options for the FileLoggerProvider.</summary>
    public class FileLoggerOptions : BatchingLoggerOptions
    {
        private string _FileNameRoot;
        private string _LogDirectory;
        private int _MaximumFileCount;
        private int _MaximumFileSize;

        /// <summary>
        /// The root name of the file that log entries will be written to.</summary>
        /// <remarks>
        /// <para>The date and time the file was created and a file extension will be appended to this file file name root.</para>
        /// <para>This file name must not contain a path component.</para>
        /// </remarks>
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

        /// <summary>
        /// The directory where log files will be written.</summary>
        /// <remarks>
        /// <para>The logger provider will also limit the number files that are saved in this directory.</para>
        /// </remarks>
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

        /// <summary>
        /// The maximum number of files that will be saved.</summary>
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

        /// <summary>
        /// The maximum size of a log file.</summary>
        /// <remarks>
        /// <para>Files will often be longer than this value because the size is checked before a batch of entries are written and the complete batch
        /// of entries is written to a single file.</para>
        /// </remarks>
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
