using Dapper.CX.Attributes;
using Dapper.CX.Enums;
using Dapper.CX.Exceptions;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Dapper.CX.Extensions
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

		public static string GetIdentityName(this Type modelType)
		{
			return GetIdentityProperty(modelType).Name;
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

		public static string GetTableName(this Type modelType)
		{
			string result = modelType.Name;

			if (modelType.HasAttribute(out TableAttribute attr))
			{
				result = attr.Name;
				if (!string.IsNullOrEmpty(attr.Schema))
				{
					result = attr.Schema + "." + result;
				}
			}

			return result;
		}

		public static string GetColumnName(this PropertyInfo propertyInfo)
		{
			string result = propertyInfo.Name;

			var attr = propertyInfo.GetCustomAttribute<ColumnAttribute>();
			if (attr != null) result = attr.Name;

			return result;
		}

		public static bool IsIdentity(this PropertyInfo propertyInfo)
		{
			try
			{
				var type = propertyInfo.DeclaringType;
				return (type.TryGetIdentityName(out string name)) ? name.Equals(propertyInfo.Name) : false;
			}
			catch 
			{
				return false;
			}
		}

		public static bool AllowSaveAction(this PropertyInfo propertyInfo, SaveAction saveAction)
		{
			return
				(propertyInfo.HasAttribute(out SaveActionAttribute attr)) ? attr.SaveAction == saveAction :
				true;
		}
	}
}
