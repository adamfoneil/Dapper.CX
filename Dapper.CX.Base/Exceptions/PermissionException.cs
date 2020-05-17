using System;

namespace Dapper.CX.Exceptions
{
    public class PermissionException : Exception
    {
        public PermissionException(string message) : base(message)
        {
        }
    }
}
