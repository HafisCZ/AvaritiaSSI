using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public sealed class SqlTable
    {
        private SqlTableTemplate Template { get; }
        private SqlConnector Connector { get; }

        public String Label { get; }

        internal SqlTable(SqlConnector connector, String label, SqlTableTemplate template)
        {
            Connector = connector;
            Label = label;
            Template = template;
        }

        public void Truncate()
        {
            SqlRequest request = new SqlRequest { Statement = String.Format("DELETE FROM {0}", Label) };
            Connector.ExecuteNonQuerry(ref request, true);
        }

        public void Select<Type>(out List<Type> records, String filter, params String[] headers) where Type : ISqlUserRecord, new()
        {
            String[] requestedHeaders = headers.Length > 0 ? headers : Template.Columns.Keys.ToArray();
            Dictionary<Int32, PropertyDescriptor> descriptors = Enumerable.Range(0, TypeCache.GetDescriptors(typeof(Type)).Count).Select(iteration => new { ordinal = iteration, property = TypeCache.TryGetDescriptor(typeof(Type), requestedHeaders[iteration]) }).SkipWhile(value => value.property != null).ToDictionary(p => p.ordinal, p => p.property);

            records = new List<Type>();
           
            SqlRequest request = new SqlRequest { Statement = String.Format("SELECT {0} FROM {1}{2}", String.Join(",", requestedHeaders), Label, (filter == null) ? "" : String.Format(" WHERE {0}", filter)) };
            Connector.Execute(ref request, true, SqlConnector.DEFAULT_BEHAVIOUR);
            foreach (Object[] row in request.Response) {
                Type record = new Type();
                foreach (KeyValuePair<Int32, PropertyDescriptor> kvp in descriptors) {
                    kvp.Value?.SetValue(record, row[kvp.Key]);
                }
                records.Add(record);
            }
        }

        public void Update<Type>(ref Type record) where Type : ISqlUserRecord
        {
            Dictionary<String, Boolean> headers = new Dictionary<String, Boolean>();
            foreach (SqlColumnTemplate templates in Template.Columns.Values) {
                headers[templates.Label] = templates.PrimaryKey;
            }

            Dictionary<String, Object> vals = new Dictionary<String, Object>();
            foreach (KeyValuePair<String, Boolean> pair in headers) {
                PropertyDescriptor descriptor = TypeCache.TryGetDescriptor(typeof(Type), pair.Key);
                if (descriptor == null && pair.Value) {
                    throw new Exception();
                } else if (descriptor != null) {
                    vals[pair.Key] = descriptor.GetValue(record);
                }
            }

            SqlRequest request = new SqlRequest {
                Statement = String.Format("UPDATE {0} SET {1} WHERE {2}", Label, String.Join(",", headers.Where(pair => !pair.Value).Select(pair => String.Format("{0}='{1}'", pair.Key, vals[pair.Key]))), String.Join(",", headers.Where(pair => pair.Value).Select(pair => String.Format("{0}='{1}'", pair.Key, vals[pair.Key]))))
            };
            Connector.ExecuteNonQuerry(ref request, true);
        }

        public void Delete<Type>(ref Type record) where Type : ISqlUserRecord
        {
            String[] primaryKeys = Template.Columns.Where(pair => pair.Value.PrimaryKey).Select(pair => pair.Key).ToArray();

            Dictionary<String, Object> vals = new Dictionary<String, Object>();
            foreach (String key in primaryKeys) {
                PropertyDescriptor descriptor = TypeCache.TryGetDescriptor(typeof(Type), key);
                if (descriptor == null) {
                    throw new Exception();
                } else if (descriptor != null) {
                    vals[key] = descriptor.GetValue(record);
                }
            }

            SqlRequest request = new SqlRequest {
                Statement = String.Format("DELETE FROM {0} WHERE {1}", Label, String.Join(",", vals.Select(pair => String.Format("{0}='{1}'", pair.Key, vals[pair.Key]))))
            };
            Connector.ExecuteNonQuerry(ref request, true);
        }

        public void Insert<Type>(ref Type record) where Type : ISqlUserRecord
        {
            Dictionary<String, String> values = new Dictionary<String, String>();

            foreach (SqlColumnTemplate template in Template.Columns.Values) {
                PropertyDescriptor descriptor = TypeCache.TryGetDescriptor(typeof(Type), template.Label);
                if (descriptor == null && !template.AllowNull) {
                    throw new Exception();
                } else if (descriptor != null) {
                    values[template.Label] = String.Format("'{0}'", descriptor.GetValue(record));
                }
            }

            SqlRequest request = new SqlRequest { Statement = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", Label, String.Join(",", values.Keys), String.Join(",", values.Values)) };
            Connector.ExecuteNonQuerry(ref request, true);
        }
    }

    public interface ISqlUserRecord
    {

    }
}
