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
        private Dictionary<String, SqlRoutineTemplate> Routines { get; }

        public SqlDatabase(String connectionString, String commonPrefix = "")
        {
            Connector = new SqlConnector(connectionString);
            Tables = CreateTemplates(commonPrefix);
            Routines = CreateRoutines(commonPrefix);
        }

        public void Dispose()
        {
            Connector.Dispose();
        }

        private Dictionary<String, SqlRoutineTemplate> CreateRoutines(String prefix)
        {
            Logger.Log(Logger.Action.TEMPLATE_ROUTINE_MAP_BEGIN, prefix);
            Dictionary<String, SqlRoutineTemplate> routine = new Dictionary<String, SqlRoutineTemplate>();

            SqlRequest request = new SqlRequest { Statement = String.Format("SELECT ROUTINE_NAME, ROUTINE_TYPE FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME LIKE '{0}%'", prefix) };
            Connector.Execute(ref request, true);
            foreach (Object[] row in request.Response) {
                routine[row[0] as String] = new SqlRoutineTemplate(row[0] as String, (row[1] as String).Equals("PROCEDURE"));
            }

            Logger.Log(Logger.Action.TEMPLATE_ROUTINE_MAP_END);
            return routine;
        }

        private Dictionary<String, SqlTableTemplate> CreateTemplates(String prefix)
        {
            Logger.Log(Logger.Action.TEMPLATE_MAP_BEGIN, prefix);
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

            Logger.Log(Logger.Action.TEMPLATE_MAP_END);
            return tables;
        }

        public SqlRoutine GetRoutine(String routine)
        {
            try {
                return new SqlRoutine(Connector, Routines[routine]);
            } catch (Exception) {
                Logger.Log(Logger.Action.TEMPLATE_NOTFOUND);
                return null;
            }
        }

        public String[] GetRoutines(Boolean filterFunctions = true, Boolean filterProcedures = true)
        {
            try {
                return Routines.Where(routine => (routine.Value.IsFunction && filterFunctions) || (routine.Value.IsProcedure && filterProcedures)).Select(template => template.Key).ToArray();
            } catch (Exception) {
                Logger.Log();
                return null;
            }
        }


        public SqlTable GetTable(String table)
        {
            try {
                return new SqlTable(Connector, Tables[table]);
            } catch (Exception) {
                Logger.Log(Logger.Action.TEMPLATE_NOTFOUND);
                return null;
            }
        }

        public String[] GetTables()
        {
            try {
                return Tables.Select(template => template.Key).ToArray();
            } catch (Exception) {
                Logger.Log();
                return null;
            }
        }

        public void Begin() => Connector.TransactionPackBegin();
        public void End() => Connector.TransactionPackEnd();
    }
}
