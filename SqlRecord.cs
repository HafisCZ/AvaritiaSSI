using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public interface ISqlUserRecord
    {
        String GetTableLabel();
    }

    public class SqlDynamicRecord : DynamicObject
    {
        private readonly Dictionary<String, Object> properties = new Dictionary<String, Object>();

        public String TableName { get; }

        public Object this[String property] {
            set => properties[property] = value;
            get => properties[property];
        }

        public SqlDynamicRecord(String table) : base() => TableName = table;
        public override IEnumerable<String> GetDynamicMemberNames() => properties.Keys;
        public Boolean HasProperty(String property) => properties.ContainsKey(property);

        public override bool TryGetMember(GetMemberBinder binder, out object result) => properties.TryGetValue(binder.Name, out result);
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (properties.ContainsKey(binder.Name)) {
                properties[binder.Name] = value;
                return true;
            } else {
                return false;
            }
        }
    }
}
