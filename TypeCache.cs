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

        public static PropertyDescriptor TryGetDescriptor(Type type, String name)
        {
            Dictionary<String, PropertyDescriptor> descriptors = GetDescriptors(type);
            return descriptors.ContainsKey(name) ? descriptors[name] : null;
        }

        public static Dictionary<String, PropertyDescriptor> GetDescriptors(Type type)
        {
            if (!Descriptors.ContainsKey(type)) {
                Cache(type);
            }

            return Descriptors[type];
        }

        public static void Cache(params Type[] types)
        {
            foreach (Type type in types) {
                Logger.Log(Logger.TYPECACHE_INSERT);

                PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(type);
                Descriptors.Add(type, Enumerable.Range(0, descriptors.Count).ToDictionary(i => descriptors[i].Name, i => descriptors[i]));
            }
        }
    }
}
