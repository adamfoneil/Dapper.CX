using Dapper.CX.Base.Attributes;
using Dapper.CX.Base.Exceptions;
using System;
using System.Reflection;

namespace Dapper.CX.Base.Extensions
{
	public static class ModelClassHelper
    {
		public const string DefaultIdentityProperty = "Id";

        public static PropertyInfo GetIdentityProperty(this Type modelType)
        {
			try
			{
				return
					(modelType.HasAttribute(out IdentityAttribute attr)) ? modelType.GetProperty(attr.PropertyName) :
					(modelType.HasProperty(DefaultIdentityProperty, out PropertyInfo identityProp)) ? identityProp :
					throw new IdentityException(modelType, $"Couldn't find an identity property on type {modelType.FullName}");
            }
			catch (Exception exc)
			{
				throw new IdentityException(modelType, exc);
			}
        }

		public static bool TryGetIdentityName(this Type modelType, out string propertyName)
		{
			try
			{
				propertyName = GetIdentityProperty(modelType).Name;
				return true;
			}
			catch 
			{
				propertyName = null;
				return false;
			}
		}
    }
}
