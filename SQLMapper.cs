using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public class SQLMapper
    {
        private enum SQLVirtualType
        {
            TABLE, VARIABLE, ROOT
        }

        private class SQLVirtualVariableParams
        {
            public int? Ordinal { get; set; }
            public string Default { get; set; }
            public bool? Nullable { get; set; }
            public string Type { get; set; }
            public int? Length { get; set; }

            public SQLVirtualVariableParams(object[] column)
            {
                Ordinal = column[4] as int?;
                Default = column[5] as string;
                Nullable = column[6] as bool?;
                Type = column[7] as string;
                Length = column[8] as int?;
            }
        }

        private class SQLVirtualMapNode
        {
            public SQLVirtualType Type { get; set; }
            public string Label { get; set; }
            public SQLVirtualVariableParams Parameters { get; set; }
            public List<SQLVirtualMapNode> Nodes { get; } = new List<SQLVirtualMapNode>();
        }

        private SQLHook SQLHook { get; set; }
        private SQLVirtualMapNode VirtualMap { get; set; }

        public SQLMapper(SQLHook sqlh)
        {
            SQLHook = sqlh;

            try {
                CreateVirtualMap();
            } catch (Exception) {
                throw new SQLCannotCreateVirtualMapException();
            }
        }

        public void CreateVirtualMap()
        {
            SQLVirtualMapNode root = new SQLVirtualMapNode { Type = SQLVirtualType.ROOT, Label = "" };

            SQLRequest request = new SQLRequest { Request = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME LIKE 'table_%'" };
            SQLHook.Read(ref request);

            foreach (object[] record in request.Response) {
                SQLVirtualMapNode table = new SQLVirtualMapNode { Type = SQLVirtualType.TABLE, Label = record[0].ToString() };

                SQLRequest request2 = new SQLRequest { Request = String.Format("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", table.Label) };
                SQLHook.Read(ref request2);

                request2.Response.ForEach(rec => table.Nodes.Add(new SQLVirtualMapNode { Type = SQLVirtualType.VARIABLE, Label = rec[3].ToString(), Parameters = new SQLVirtualVariableParams(rec) }));

                root.Nodes.Add(table);
            }

            VirtualMap = root;
        }

        public List<Record> ReadRecords(string tablename, string sqlq)
        {
            SQLRequest request = new SQLRequest { Request = sqlq };
            SQLHook.Read(ref request);

            List<Record> records = new List<Record>();

            foreach (object[] raw in request.Response) {
                Record record = new Record();

                SQLVirtualMapNode table = VirtualMap.Nodes.Find(tab => tab.Label == tablename && tab.Type == SQLVirtualType.TABLE);
                if (table == null) {
                    throw new SQLVirtualMapFetchException();
                }

                for (int i = 0; i < raw.Length; i++) {
                    string columnType = table.Nodes[i].Parameters.Type;

                    if (columnType == "int") {
                        record.Values.Add(new SQLInteger((int) raw[i]));
                    } else if (columnType == "varchar") {
                        record.Values.Add(new SQLString((string) raw[i]));
                    } else if (columnType == "date") {
                        record.Values.Add(new SQLDate((DateTime) raw[i]));
                    }
                }

                records.Add(record);
            }

            return records;
        }
    }
}