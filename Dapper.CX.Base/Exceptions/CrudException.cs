using System;
using System.Collections.Generic;

namespace Dapper.CX.Base.Exceptions
{
    public class CrudException : Exception
    {
        public CrudException(Dictionary<string, object> record, Exception innerException) : base(innerException.Message, innerException)
        {
            Record = record;
        }

        public Dictionary<string, object> Record { get; }
    }
}
