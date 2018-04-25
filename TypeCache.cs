using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public static class TypeCache
    {
        private static readonly Dictionary<Type, Dictionary<String, PropertyDescriptor>> Descriptors = new Dictionary<Type, Dictionary<String, PropertyDescriptor>>();

        public static PropertyDescriptor GetDescriptor(Type type, String propertyName)
        {
            if (GetDescriptors(type).TryGetValue(propertyName, out PropertyDescriptor propertyDescriptor)) {
                return propertyDescriptor;
            } else {
                Logger.Log(Logger.Action.TYPECACHE_FAIL, propertyName);

                return null;
            }
        }

        public static Dictionary<String, PropertyDescriptor> GetDescriptors(Type type)
        {
            if (Descriptors.TryGetValue(type, out Dictionary<String, PropertyDescriptor> propertyDescriptors)) {
                return propertyDescriptors;
            } else {
                Logger.Log(Logger.Action.TYPECACHE_MAP, type.ToString());

                PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(type);
                Descriptors.Add(type, Enumerable.Range(0, descriptors.Count).ToDictionary(i => descriptors[i].Name, i => descriptors[i]));

                return Descriptors[type];
            }
        }
    }
}
