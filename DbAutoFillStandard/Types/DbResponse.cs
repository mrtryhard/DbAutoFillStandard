using System;
using System.Collections.Generic;

namespace DbAutoFillStandard.Types
{
    public sealed class DbResponse<T>
    {
        public bool HasResult
        {
            get { return ResultSet.Count > 0; }
        }

        public bool HasError
        {
            get { return InnerException != null || !string.IsNullOrWhiteSpace(ErrorMessage); }
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public Exception InnerException
        {
            get;
            private set;
        }

        public IList<T> ResultSet
        {
            get;
            private set;
        }

        public DbResponse()
        {
            ResultSet = new List<T>();
        }

        public DbResponse(string errorMessage, Exception innerException)
            : this()
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                throw new ArgumentException("errorMessage cannot be empty.", "errorMessage");

            InnerException = innerException ?? throw new ArgumentNullException("ex");
            ErrorMessage = errorMessage;
        }
    }
}
