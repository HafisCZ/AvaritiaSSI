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

    internal class SqlTablePattern : Dictionary<String, SqlColumnData>
    {
        public String Label { get; internal set; }
        public String[] Headers { get; internal set; }

        public static void MakeHeaders(ref SqlTablePattern sqltp)
        {
            sqltp.Headers = new String[sqltp.Count];
            foreach (KeyValuePair<String, SqlColumnData> kvp in sqltp) {
                sqltp.Headers[kvp.Value.Ordinal] = kvp.Key;
            }
        }
    }

    internal class SqlDatabasePattern : Dictionary<String, SqlTablePattern>
    {
        public String[] Tables { get; internal set; }

        public static void MakeTables(ref SqlDatabasePattern sqldp) {
            sqldp.Tables = sqldp.Keys.ToArray();
            foreach(KeyValuePair<String, SqlTablePattern> kvp in sqldp) {
                SqlTablePattern tp = kvp.Value;
                SqlTablePattern.MakeHeaders(ref tp);
            }
        }
    }
}
