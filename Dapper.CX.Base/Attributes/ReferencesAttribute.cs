using System;

namespace Dapper.CX.Attributes
{
    /// <summary>
    /// defines a foreign key on a property by referring to the primary type
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ReferencesAttribute : Attribute
    {
        public ReferencesAttribute(Type primaryType)
        {
            PrimaryType = primaryType;
        }

        public Type PrimaryType { get; }
    }
}
