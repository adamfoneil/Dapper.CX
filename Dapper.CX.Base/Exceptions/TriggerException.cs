using System;

namespace Dapper.CX.Exceptions
{
    public enum TriggerAction
    {
        Save,
        Delete
    }

    public class TriggerException : Exception
    {
        public TriggerException(Exception innerException, TriggerAction action) : base(innerException.Message, innerException)
        {
            Action = action;
        }

        public TriggerAction Action { get; }
    }
}
