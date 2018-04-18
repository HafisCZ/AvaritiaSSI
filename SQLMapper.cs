using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public interface ISQLRecord
    {

    }

    public class SQLMapper
    {
        private class SQLVirtualMap : Dictionary<String, SQLVirtualTable>
        {
        }

        private class SQLVirtualTable : Dictionary<String, SQLVirtualColumn>
        {
            public String Label { get; set; }

            public SQLVirtualColumn this[int ordinal] {
                get {
                    foreach (KeyValuePair<string, SQLVirtualColumn> kvp in this) {
                        if (kvp.Value.Ordinal == ordinal) {
                            return kvp.Value;
                        }
                    }

                    return null;
                }
            }

            public SQLVirtualColumn[] Headers {
                get {
                    SQLVirtualColumn[] sqlvc = new SQLVirtualColumn[Count];
                    foreach (KeyValuePair<string, SQLVirtualColumn> kvp in this) {
                        sqlvc[kvp.Value.Ordinal - 1] = kvp.Value;
                    }

                    return sqlvc;
                }
            }
        }

        private class SQLVirtualColumn
        {
            public String Label { get; set; }
            public Type Datatype { get; set; }
            public int Ordinal { get; set; }
            public bool AllowNull { get; set; }
            public int? MaxLength { get; set; }

            public static SQLVirtualColumn ParseISCRecord(object[] record) {
                return new SQLVirtualColumn {
                    Label = record[3] as String,
                    Ordinal = record[4] as int? ?? 0,
                    AllowNull = record[6] as bool? ?? false,
                    MaxLength = record[8] as int?,
                    Datatype = ParseType(record[7] as String)
                };
            }

            private static Type ParseType(string type) {
                switch (type) {
                    case "int": return typeof(int);
                    case "varchar": return typeof(String);
                    case "date": return typeof(DateTime);
                    default: return null;
                }
            }
        }

        internal SQLHook Hook { get; private set; }
        private SQLVirtualMap VirtualMap { get; set; }

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
                    crequest.Response.ForEach(
                        column => {
                            SQLVirtualColumn vcolumn = SQLVirtualColumn.ParseISCRecord(column);
                            vtable[vcolumn.Label] = vcolumn;
                        }
                    );

                    map[tablename] = vtable;
                }
            );

            return map;
        }

        public void Fetch<T>(ref List<T> records, String table, params String[] columns) where T : ISQLRecord, new() {
            SQLRequest sqlr = new SQLRequest { Request = String.Format("SELECT {0} FROM {1}", String.Join(", ", columns), table) };
            Hook.Fetch(ref sqlr);

            foreach (object[] r in sqlr.Response) {
                T record = new T();
                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(record);

                for (int i = 0; i < r.Length; i++) {
                    PropertyDescriptor pd = pdc.Find(columns[i], false);
                    if (pd != null) {
                        pd.SetValue(record, r[i]);
                    }
                }

                records.Add(record);
            }
        }
    }
}