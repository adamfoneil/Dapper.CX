using Dapper.CX.Enums;
using System;

namespace Dapper.CX.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SaveActionAttribute : Attribute
    {
        public SaveActionAttribute(SaveAction saveAction)
        {
            SaveAction = saveAction;
        }

        public SaveAction SaveAction { get; }
    }
}
