using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public interface ISQLRecord
    {
        String SourceTable { get; set; }
        SQLMapper Mapper { get; set; }
    }

    public class SQLUserRecord : ISQLRecord
    {
        public String SourceTable { get; set; }
        public SQLMapper Mapper { get; set; }
    }

    public sealed class SQLAnonymousRecord : DynamicObject, ISQLRecord
    {
        public String SourceTable { get; set; }
        public SQLMapper Mapper { get; set; }

        private readonly Dictionary<String, object> m_properties = new Dictionary<string, object>();

        public void AddProperty(string propertyName, object value)
        {
            m_properties[propertyName] = value;
        }

        public override IEnumerable<String> GetDynamicMemberNames()
        {
            return m_properties.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return m_properties.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (m_properties.ContainsKey(binder.Name)) {
                m_properties[binder.Name] = value;
                return true;
            } else {
                return false;
            }
        }
    }
}
