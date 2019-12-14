using System;
using System.Linq;
using System.Reflection;

namespace Dapper.CX.Extensions
{
    internal static class ReflectionHelper
    {
        internal static bool HasAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return HasAttribute<T>(memberInfo, out _);
        }

        internal static bool HasAttribute<T>(this MemberInfo memberInfo, out T attribute) where T : Attribute
        {
            var attr = memberInfo.GetCustomAttribute(typeof(T));
            if (attr != null)
            {
                attribute = attr as T;
                return true;
            }

            attribute = null;
            return false;
        }

        internal static T GetAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            if (HasAttribute(memberInfo, out T result)) return result;
            return null;
        }

        internal static bool HasProperty(this Type type, string propertyName, out PropertyInfo propertyInfo)
        {
            var properties = type.GetProperties().ToDictionary(pi => pi.Name);
            propertyInfo = (properties.ContainsKey(propertyName)) ? properties[propertyName] : null;
            return (propertyInfo != null);
        }
    }
}
