using System.Collections.Generic;

namespace TranslatorApk.Logic.OrganisationItems
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<string, object> items = new Dictionary<string, object>();

        public static T GetInstance<T>() where T: new()
        {
            string type = typeof(T).FullName;

            if (items.TryGetValue(type, out object value))
                return (T) value;
            
            T val = new T();

            items.Add(type, val);

            return val;
        }
    }
}
