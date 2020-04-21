using System;

namespace Dapper.CX.Exceptions
{
    public class ChangeTrackerSaveException : Exception
    {
        public ChangeTrackerSaveException(Exception innerException) : base(innerException.Message, innerException)
        {
        }
    }
}
