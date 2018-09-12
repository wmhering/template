using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CuyahogaHHS.Logging
{
    public class FileLoggerProvider : BatchingLoggerProvider
    {
        private readonly string _FileNameRoot;
        private readonly string _LogDirectory;
        private readonly int _MaximumFileCount;
        private readonly int _MaximumFileSize;

        public FileLoggerProvider(IOptions<FileLoggerOptions> options, IConfiguration configuration) : base(options, configuration)
        {
            var loggerOptions = options.Value;
            _FileNameRoot = loggerOptions.FileNameRoot;
            _LogDirectory = loggerOptions.LogDirectory;
            _MaximumFileCount = loggerOptions.MaximumFileCount;
            _MaximumFileSize = loggerOptions.MaximumFileSize;
        }

        protected override async Task WriteLogEntriesAsync(IEnumerable<LogEntry> messages, CancellationToken cancellationToken)
        {
            Stream stream = null;
            TextWriter writer = null;
            Directory.CreateDirectory(_LogDirectory);
            try
            {
                foreach (var message in messages)
                {
                    if (writer == null)
                    {
                        stream = OpenFile();
                        writer = new StreamWriter(stream);
                    }
                    await writer.WriteLineAsync();
                    await writer.FlushAsync();
                    if (stream.Position >= _MaximumFileSize)
                    {
                        writer.Close();
                        writer.Dispose();
                        stream.Dispose();
                        writer = null;
                        stream = null;
                    }
                }
            }
            finally
            {
                if (writer !=null)
                {
                    await writer.FlushAsync();
                    writer.Close();
                    writer.Dispose();
                }
                if (stream!=null)
                {
                    stream.Dispose();
                }
            }
            RemoveOldFiles();
        }

        private Stream OpenFile()
        {
            var fileInfo = new DirectoryInfo(_LogDirectory)
                .GetFiles(_FileNameRoot + "*.log")
                .OrderByDescending(f => f.Name)
                .FirstOrDefault(f => f.Length < _MaximumFileSize);
            Stream result = new FileStream(fileInfo.FullName, FileMode.Append, FileAccess.Write, FileShare.Read);
            if (result == null)
                result = OpenNewFile();
            return result;
        }

        private Stream  OpenNewFile()
        {
            var path = Path.Combine(_LogDirectory, $"{_FileNameRoot}{DateTime.Now:yyyyMMddHHmmss}.log");
            var result = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            return result;
        }

        protected void RemoveOldFiles()
        {
            if (_MaximumFileCount > 0)
            {
                var files = new DirectoryInfo(_LogDirectory)
                    .GetFiles(_FileNameRoot + "*.log")
                    .OrderByDescending(f => f.Name)
                    .Skip(_MaximumFileCount);
                foreach (var file in files)
                    file.Delete();
            }
        }
    }
}
