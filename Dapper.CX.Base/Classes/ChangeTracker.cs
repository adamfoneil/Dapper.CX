using AO.Models.Attributes;
using AO.Models.Enums;
using Dapper.CX.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dapper.CX.Classes
{
    public class ChangeTracker<TModel> : Dictionary<string, object>
    {
        private readonly Dictionary<string, PropertyInfo> _properties;
        private readonly IEnumerable<string> _ignoreProps;

        public TModel Instance { get; set; }

        public ChangeTracker(TModel @object)
        {
            Instance = @object;

            string identityName = (typeof(TModel).TryGetIdentityName(out string name)) ? name : string.Empty;

            var props = @object.GetType().GetProperties().Where(pi =>
                pi.CanWrite &&
                !pi.GetIndexParameters().Any() &&
                !pi.Name.Equals(identityName)).ToArray();

            _ignoreProps = props.Where(pi => pi.HasAttribute<NoChangeTrackingAttribute>()).Select(pi => pi.Name);
            _properties = props.ToDictionary(pi => pi.Name);

            foreach (var pi in props) Add(pi.GetColumnName(), pi.GetValue(@object));
        }

        /// <summary>
        /// call this after you've made desired changes to your model class instance to get the names of modified properties
        /// </summary>
        public string[] GetModifiedColumns(SaveAction? saveAction = null)
        {
            return GetModifiedProperties(saveAction)
                .Select(kp => kp.Key)
                .ToArray();
        }

        protected IEnumerable<KeyValuePair<string, PropertyInfo>> GetModifiedProperties(SaveAction? saveAction = null, bool loggableOnly = false)
        {
            Func<KeyValuePair<string, PropertyInfo>, bool> filter = (kp) => IsModified(kp, Instance);

            if (saveAction.HasValue)
            {
                filter = (kp) => IsModified(kp, Instance) && kp.Value.AllowSaveAction(saveAction.Value);
            }

            return _properties.Where(kp => filter(kp) && isLogged(kp));

            bool isLogged(KeyValuePair<string, PropertyInfo> kp)
            {
                // if not excluding ignore props, then include this property
                if (!loggableOnly) return true;

                // otherwise, don't log ignored properties
                return !_ignoreProps.Contains(kp.Key);
            }
        }

        private bool IsModified(KeyValuePair<string, PropertyInfo> kp, TModel @object)
        {
            // when using logged change tracker, there might be ignored properties that we need to manually exclude here
            if (!ContainsKey(kp.Key)) return false;

            var value = kp.Value.GetValue(@object);
            return
                (value == null ^ this[kp.Key] == null) ? true :
                (value == null && this[kp.Key] == null) ? false :
                !value.Equals(this[kp.Key]);            
        }
    }
}
