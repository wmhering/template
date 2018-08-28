using System;
using System.Collections.Generic;
using System.Text;

namespace CuyahogaHHS.Bll
{
    public class ConcurrencyResult<T> where T : class
    {
        public ConcurrencyResult(T data, bool concurrencyError = false)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            ConcurrencyError = concurrencyError;
        }

        public bool ConcurrencyError { get; }

        public T Data { get; }
    }
}
