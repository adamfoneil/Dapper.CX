using System;

namespace Dapper.CX.Base.Exceptions
{
    public class CrudException : Exception
    {
        public CrudException(CommandDefinition commandDef, Exception innerException) : base(innerException.Message, innerException)
        {
            CommandDefinition = commandDef;
        }

        public CommandDefinition CommandDefinition { get; }
    }
}
