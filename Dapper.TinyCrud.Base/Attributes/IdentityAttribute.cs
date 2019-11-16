using System;

namespace Dapper.TinyCrud.Base.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IdentityAttribute : Attribute
    {        
        public IdentityAttribute(string columnName)
        {
            ColumnName = columnName;
        }

        public string ColumnName { get; }
    }
}
