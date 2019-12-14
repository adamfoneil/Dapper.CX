using Dapper.CX.Base.Enums;
using System;

namespace Dapper.CX.Base.Attributes
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
