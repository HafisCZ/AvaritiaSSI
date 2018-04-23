using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public sealed class SqlDatabase
    {
        private SqlConnector Connector { get; }

        private Dictionary<String, SqlTableTemplate> Tables { get; }

        public SqlDatabase(String connectionString, String commonPrefix)
        {
            Connector = new SqlConnector(connectionString);
            Tables = CreateTemplates(commonPrefix);
        }

        public void Dispose()
        {
            Connector.Dispose();
        }

        private Dictionary<String, SqlTableTemplate> CreateTemplates(String prefix)
        {
            Logger.Log(Logger.TEMPLATE_CREATE);
            Dictionary<String, SqlTableTemplate> tables = new Dictionary<String, SqlTableTemplate>();

            SqlRequest request = new SqlRequest { Statement = String.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME LIKE '{0}%'", prefix) };
            Connector.Execute(ref request, true);
            foreach (Object[] row in request.Response) {
                String label = row[0] as String;
                tables.Add(label, new SqlTableTemplate(label));
            }

            request = new SqlRequest { Statement = String.Format("SELECT TABLE_NAME, COLUMN_NAME, ORDINAL_POSITION, IS_NULLABLE, COLUMN_DEFAULT FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME LIKE '{0}%'", prefix) };
            Connector.Execute(ref request, true);
            foreach (Object[] row in request.Response) {
                tables[row[0] as String].AddColumn(new SqlColumnTemplate(row[1] as String, (row[2] as Int32? ?? 0) - 1, false, row[3] as Boolean? ?? false, row[4] != null));
            }

            request = new SqlRequest { Statement = "SELECT TC.TABLE_NAME, CCU.COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC LEFT JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE CCU ON TC.CONSTRAINT_NAME = CCU.CONSTRAINT_NAME WHERE TC.CONSTRAINT_TYPE = 'PRIMARY KEY'" };
            Connector.Execute(ref request, true);
            foreach (Object[] row in request.Response) {
                if (tables.ContainsKey(row[0] as String)) {
                    tables[row[0] as String].Columns[row[1] as String].MarkAsPrimary();
                }
            }

            request = new SqlRequest { Statement = "SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMNPROPERTY(object_id(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsIdentity') = 1" };
            Connector.Execute(ref request, true);
            foreach (Object[] row in request.Response) {
                if (tables.ContainsKey(row[0] as String)) {
                    tables[row[0] as String].Columns[row[1] as String].MarkAsDefault();
                }
            }

            Logger.Log(Logger.TEMPLATE_FINISHED);
            return tables;
        }

        public SqlTable GetTable(String table)
        {
            try {
                return new SqlTable(Connector, table, Tables[table]);
            } catch (Exception) {
                Logger.Log(Logger.TEMPLATE_DOES_NOT_EXIST);
                return null;
            }
        }
    }
}
