using System;

namespace Dapper.CX.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UniqueConstraintAttribute : Attribute
    {
        public UniqueConstraintAttribute(params string[] propertyNames)
        {
            PropertyNames = propertyNames;
        }

        public string[] PropertyNames { get; }
    }
}
