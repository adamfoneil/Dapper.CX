using System;

namespace Dapper.CX.Exceptions
{
    public class IdentityException : Exception
    {
        public IdentityException(Type modelType, string message) : base(message)
        {
            ModelType = modelType;
        }

        public IdentityException(Type modelType, Exception innerException) : base(innerException.Message, innerException)
        {
            ModelType = modelType;
        }

        public Type ModelType { get; }
    }
}
