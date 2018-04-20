using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    internal class SqlColumnData
    {
        public Int32 Ordinal { get; internal set; }
        public String Label { get; internal set; }
        public Boolean PrimaryKey { get; internal set; } = false;
        public Boolean ForeignKey { get; internal set; } = false;
        public Boolean AllowNull { get; internal set; }
        public Object Default { get; internal set; }
        public String Type { get; internal set; }
        public Int32? Length { get; internal set; }
    }

    internal class SqlTableTemplate : Dictionary<String, SqlColumnData>
    {
        public String Label { get; internal set; }
        public String[] Headers { get; internal set; }

        public static void MakeHeaders(ref SqlTableTemplate sqltp)
        {
            sqltp.Headers = new String[sqltp.Count];
            foreach (KeyValuePair<String, SqlColumnData> kvp in sqltp) {
                sqltp.Headers[kvp.Value.Ordinal] = kvp.Key;
            }
        }
    }

    internal class SqlDatabaseTemplate : Dictionary<String, SqlTableTemplate>
    {
        public String[] Tables { get; internal set; }

        public static void MakeTables(ref SqlDatabaseTemplate sqldp) {
            sqldp.Tables = sqldp.Keys.ToArray();
            foreach(KeyValuePair<String, SqlTableTemplate> kvp in sqldp) {
                SqlTableTemplate tp = kvp.Value;
                SqlTableTemplate.MakeHeaders(ref tp);
            }
        }
    }
}
