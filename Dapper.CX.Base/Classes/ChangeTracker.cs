using Dapper.CX.Base.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dapper.CX.Base.Classes
{
    public class ChangeTracker<TModel> : Dictionary<string, object>
    {
        private readonly Dictionary<string, PropertyInfo> _properties;
        private readonly TModel _instance;

        public ChangeTracker(TModel @object)
        {
            _instance = @object;

            string identityName = (typeof(TModel).TryGetIdentityName(out string name)) ? name : string.Empty;
            var props = @object.GetType().GetProperties().Where(pi =>
                pi.CanWrite &&
                !pi.GetIndexParameters().Any() &&
                !pi.Name.Equals(identityName)).ToArray();
            _properties = props.ToDictionary(pi => pi.Name);
            foreach (var pi in props) Add(pi.GetColumnName(), pi.GetValue(@object));
        }

        /// <summary>
        /// call this after you've made desired changes to your model class instance to get the names of modified properties
        /// </summary>
        public string[] GetModifiedColumns()
        {
            return _properties
                .Where(kp => IsModified(kp, _instance))
                .Select(kp => kp.Key)
                .ToArray();
        }

        private bool IsModified(KeyValuePair<string, PropertyInfo> kp, TModel @object)
        {
            var value = kp.Value.GetValue(@object);
            return
                (value == null ^ this[kp.Key] == null) ? true :
                (value == null && this[kp.Key] == null) ? false :
                !value.Equals(this[kp.Key]);
        }
    }
}
