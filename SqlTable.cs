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
        public String Label { get; }
        private SqlConnector Connector { get; }
        private SqlTableTemplate Template { get; }

        internal SqlTable(SqlConnector connector, SqlTableTemplate template)
        {
            Connector = connector;
            Label = template.Label;
            Template = template;
        }

        public Boolean Truncate()
        {
            SqlRequest request = new SqlRequest { Statement = String.Format("DELETE FROM [{0}]", Label) };
            return Connector.Execute(ref request, true);
        }

        public Boolean Select<Type>(out List<Type> records, String filter = null) where Type : ISqlUserRecord, new()
        {
            Dictionary<Int32, PropertyDescriptor> descriptors = TypeCache.GetDescriptors(typeof(Type)).ToDictionary(pair => Template.Columns[pair.Key].Ordinal, pair => pair.Value);
            String[] headers = TypeCache.GetDescriptors(typeof(Type)).Select(pair => pair.Key).ToArray();

            records = new List<Type>();

            SqlRequest request = new SqlRequest { Statement = String.Format("SELECT {0} FROM [{1}] {2}", String.Join(",", descriptors.Select(pair => String.Format("[{0}]", headers[pair.Key]))), Label, (filter ?? "")) };
            Boolean result = Connector.Execute(ref request, false);
            foreach (Object[] row in request.Response) {
                Type record = new Type();
                foreach (KeyValuePair<Int32, PropertyDescriptor> kvp in descriptors) {
                    if (row[kvp.Key].GetType() != typeof(DBNull)) {
                        kvp.Value?.SetValue(record, row[kvp.Key]);
                    }
                }
                records.Add(record);
            }

            return result;
        }

        public Boolean Update<Type>(Type record) where Type : ISqlUserRecord
        {
            Dictionary<String, Boolean> headers = Template.Columns.Values.ToDictionary(template => template.Label, template => template.IsPrimary);

            Dictionary<String, String> pairs = new Dictionary<String, String>();

            foreach (KeyValuePair<String, Boolean> pair in headers) {
                Object value = TypeCache.GetDescriptor(typeof(Type), pair.Key)?.GetValue(record);
                if (value == null && pair.Value) {
                    return false;
                } else if (value != null) {
                    pairs[String.Format("{0}", pair.Key)] = Parse(value);
                }
            }

            SqlRequest request = new SqlRequest { Statement = String.Format("UPDATE [{0}] SET {1} WHERE {2}", Label,
                String.Join(",", headers.Where(key => pairs.ContainsKey(key.Key) && !key.Value).Select(key => String.Format("[{0}]={1}", key.Key, pairs[key.Key]))), 
                String.Join(" AND ", headers.Where(key => pairs.ContainsKey(key.Key) &&  key.Value).Select(key => String.Format("[{0}]={1}", key.Key, pairs[key.Key]))))
            };
 
            return Connector.Execute(ref request, true);
        }

        public Boolean Delete<Type>(Type record) where Type : ISqlUserRecord
        {
            String[] keys = Template.Columns.Where(pair => pair.Value.IsPrimary).Select(pair => pair.Key).ToArray();
            if (keys.Length < 1) {
                keys = Template.Columns.Keys.ToArray();
            }

            Dictionary<String, String> pairs = new Dictionary<String, String>();

            foreach (String key in keys) {
                Object value = TypeCache.GetDescriptor(typeof(Type), key)?.GetValue(record);
                if (value != null || (Template.Columns[key].AllowsNull && !Template.Columns[key].IsPrimary)) {
                    pairs[String.Format("[{0}]", key)] = Parse(value);
                } else {
                    return false;
                }
            }

            SqlRequest request = new SqlRequest { Statement = String.Format("DELETE FROM [{0}] WHERE {1}", Label, String.Join(" AND ", pairs.Select(pair => String.Format("{0}={1}", pair.Key, pairs[pair.Key])))) };
            return Connector.Execute(ref request, true);
        }

        public Boolean Insert<Type>(Type record) where Type : ISqlUserRecord
        {
            Dictionary<String, String> pairs = new Dictionary<String, String>();

            foreach (SqlColumnTemplate template in Template.Columns.Values) {
                PropertyDescriptor descriptor = TypeCache.GetDescriptor(typeof(Type), template.Label);
                Object value = descriptor?.GetValue(record);
                if (value != null) {
                    pairs[String.Format("[{0}]", template.Label)] = Parse(value);
                } else if (template.HasDefault || template.AllowsNull) {
                    continue;
                } else {
                    return false;
                }
            }

            SqlRequest request = new SqlRequest { Statement = String.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", Label, String.Join(",", pairs.Keys), String.Join(",", pairs.Values)) };
            return Connector.Execute(ref request, true);
        }

        public String[] GetRowLabels()
        {
            return Template.Columns.Keys.ToArray();
        }

        internal String Parse(Object o)
        {
            if (o.GetType() == typeof(DateTime)) {
                return String.Format("'{0}'", (o as DateTime?)?.Date.ToString("MM-dd-yyyy"));
            } else if (o.GetType() == typeof(String)) {
                return String.Format("'{0}'", (o as String).Replace("'", "''"));
            } else {
                return String.Format("'{0}'", o);
            }
        }

        public static Type MapOnto<Type>(SqlTable table, params Object[] values) where Type : ISqlUserRecord, new()
        {
            Type record = new Type();
            foreach (KeyValuePair<Int32, PropertyDescriptor> kvp in TypeCache.GetDescriptors(typeof(Type)).ToDictionary(pair => table.Template.Columns[pair.Key].Ordinal, pair => pair.Value)) {
                if (values[kvp.Key] != null) {
                    if (kvp.Value?.PropertyType == typeof(DateTime?)) {
                        kvp.Value?.SetValue(record, DateTime.Parse(values[kvp.Key] as String));
                    } else if (kvp.Value?.PropertyType == typeof(Int32?)) {
                        kvp.Value?.SetValue(record, Int32.Parse(values[kvp.Key] as String));
                    } else {
                        kvp.Value?.SetValue(record, values[kvp.Key]);
                    }
                }
            }

            return record;
        }
    }

    public interface ISqlUserRecord
    {

    }
}
