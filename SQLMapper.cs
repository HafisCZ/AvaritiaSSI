using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public class SQLMapper
    {
        public class SQLVirtualMap : Dictionary<String, SQLVirtualTable>
        {
            public new SQLVirtualTable this[string key] {
                set {
                    value.Label = key;
                    base[key] = value;
                }
                get {
                    return base[key];
                }
            }
        }

        public class SQLVirtualTable : Dictionary<int, Tuple<string, string>>
        {
            public String Label { get; set; }

            private String[] m_headers = null;

            public String[] Headers {
                get {
                    if (m_headers == null) {
                        m_headers = new String[Count];
                        foreach (KeyValuePair<int, Tuple<string, string>> kvp in this) {
                            m_headers[kvp.Key] = kvp.Value.Item1;
                        }
                    }

                    return m_headers;
                }
            }
        }

        internal SQLHook Hook { get; private set; }
        internal SQLVirtualMap VirtualMap { get; set; }

        public SQLMapper(SQLHook sqlh) {
            Hook = sqlh;

            try {
                VirtualMap = Map();
            } catch (Exception) {
                throw new Exception("SQLCannotCreateVirtualMapException");
            }
        }

        private SQLVirtualMap Map() {
            SQLVirtualMap map = new SQLVirtualMap();

            SQLRequest trequest = new SQLRequest { Request = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME LIKE 'table_%'" };
            Hook.Fetch(ref trequest);
            trequest.Response.ForEach(
                table => {
                    String tablename = table[0].ToString();
                    SQLVirtualTable vtable = new SQLVirtualTable { Label = tablename };

                    SQLRequest crequest = new SQLRequest { Request = String.Format("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", tablename) };
                    Hook.Fetch(ref crequest);

                    crequest.Response.ForEach( column => vtable[(column[4] as int? ?? 0) - 1] = new Tuple<string, string>(column[3] as String, column[7] as String) );

                    map[tablename] = vtable;
                }
            );

            return map;
        }

        public void Fetch<T>(ref List<T> records, String table, params String[] columns) where T : ISQLRecord, new() {
            String[] expandedColumns = (columns.Length < 1 ? VirtualMap[table].Headers : columns);

            SQLRequest sqlr = new SQLRequest { Request = String.Format("SELECT {0} FROM {1}", String.Join(", ", expandedColumns), table) };
            Hook.Fetch(ref sqlr);
            
            foreach (object[] r in sqlr.Response) {
                T record = new T { SourceTable = table, Mapper = this };
                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(record);
                
                if (record is SQLAnonymousRecord) {
                    for (int i = 0; i < r.Length; i++) {
                        (record as SQLAnonymousRecord).AddProperty(expandedColumns[i], r[i]);
                    }
                } else {
                    for (int i = 0; i < r.Length; i++) {
                        PropertyDescriptor pd = pdc.Find(expandedColumns[i], false);
                        if (pd != null) {
                            pd.SetValue(record, r[i]);
                        }
                    }
                }

                records.Add(record);
            }
        }

        public void ShowVirtualMap()
        {
            foreach (KeyValuePair<string, SQLVirtualTable> kvp in VirtualMap) {
                Console.WriteLine(String.Format("\nTABLE: {0}\n", kvp.Key));
                foreach (KeyValuePair<int, Tuple<string, string>> kvp2 in kvp.Value) {
                    Console.WriteLine(String.Format("{0,10}{1,10}{2,20}", kvp2.Key, kvp2.Value.Item2, kvp2.Value.Item1));
                }
            }
        }
    }
}