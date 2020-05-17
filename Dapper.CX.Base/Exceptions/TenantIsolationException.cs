using System;

namespace Dapper.CX.Exceptions
{
    public class TenantIsolationException : Exception
    {
        public TenantIsolationException(string message) : base(message)
        {
        }
    }
}
