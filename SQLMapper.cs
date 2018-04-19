using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Avaritia
{
    public class SqlMapper
    {
        internal SqlConnector SqlConnector { get; private set; }
        internal SqlDatabasePattern SqlDatabasePattern { get; private set; }

        public SqlMapper(SqlConnector sqlc, String tpref = null) {
            SqlConnector = sqlc;
            SqlDatabasePattern = GetDatabasePattern(sqlc, tpref);
        }

        internal static SqlDatabasePattern GetDatabasePattern(SqlConnector sqlc, String tpref)
        {
            SqlDatabasePattern sqldp = new SqlDatabasePattern();
            ISqlRequest request = SqlNonQuerryRequest.MakeTableDefinitionRequest(tpref);
            SqlConnector.Execute(sqlc, ref request);

            foreach (Object[] row in (request as SqlQuerryRequest).Response) {
                sqldp[row[0] as String] = new SqlTablePattern { Label = row[0] as String };
            }

            request = SqlNonQuerryRequest.MakeColumnDefinitionRequest(tpref);
            SqlConnector.Execute(sqlc, ref request);

            foreach (Object[] row in (request as SqlQuerryRequest).Response) {
                sqldp[row[0] as String][row[1] as String] = new SqlColumnData {
                    Label = row[1] as String,
                    Ordinal = (row[2] as Int32? ?? 0) - 1,
                    Default = row[3],
                    AllowNull = row[4] as Boolean? ?? false,
                    Type = row[5] as String,
                    Length = row[6] as Int32?
                };
            }

            ISqlRequest sqlqrP = SqlNonQuerryRequest.MakeTableConstraintRequest();
            SqlConnector.Execute(sqlc, ref sqlqrP);
            foreach (Object[] o in (sqlqrP as SqlQuerryRequest).Response) {
                sqldp[o[0] as String][o[1] as String].PrimaryKey = o[2] as String == "PRIMARY KEY";
                sqldp[o[0] as String][o[1] as String].ForeignKey = o[2] as String == "FOREIGN KEY";
            }

            SqlDatabasePattern.MakeTables(ref sqldp);
            return sqldp;
        }

        public static void Select<Type>(SqlMapper sqlm, out List<Type> list, String table, params String[] cols) where Type : ISqlUserRecord, new() => Select(sqlm, out list, table, null, cols);
        public static void Select<Type>(SqlMapper sqlm, out List<Type> list, String table, String where, params String[] cols) where Type : ISqlUserRecord, new()
        {
            String[] header = cols.Length > 0 ? sqlm.SqlDatabasePattern[table].Headers : cols;
            ISqlRequest sqlqr = SqlNonQuerryRequest.MakeSelectRequest(table, header, where);
            SqlConnector.Execute(sqlm.SqlConnector, ref sqlqr);

            list = new List<Type>();

            foreach (Object[] row in (sqlqr as SqlQuerryRequest).Response) {
                Type sqlur = new Type();

                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(sqlur);
                for (Int32 ordinal = 0; ordinal < row.Length; ordinal++) {
                    PropertyDescriptor pd = pdc.Find(header[ordinal], false);
                    if (pd != null) {
                        pd.SetValue(sqlur, row[ordinal]);
                    }
                }

                list.Add(sqlur);
            }
        }

        public static void Select(SqlMapper sqlm, out List<SqlDynamicRecord> list, String table, params String[] cols) {
            String[] header = cols.Length > 0 ? cols : sqlm.SqlDatabasePattern[table].Headers;
            ISqlRequest sqlqr = SqlNonQuerryRequest.MakeSelectRequest(table, header);
            SqlConnector.Execute(sqlm.SqlConnector, ref sqlqr);

            list = new List<SqlDynamicRecord>();

            foreach (Object[] row in (sqlqr as SqlQuerryRequest).Response) {
                SqlDynamicRecord dr = new SqlDynamicRecord(table);

                for (Int32 ordinal = 0; ordinal < row.Length; ordinal++) {
                    dr[header[ordinal]] = row[ordinal];
                }

                list.Add(dr);
            }
        }

        public static void Update<Type>(SqlMapper sqlm, ref List<Type> list, String table) where Type : ISqlUserRecord {
            String[] headers = sqlm.SqlDatabasePattern[table].Headers;

            // TODO
        }

        public static void Update(SqlMapper sqlm, ref List<SqlDynamicRecord> list, String table) {
            String[] headers = sqlm.SqlDatabasePattern[table].Headers;

            // TODO
        }

        public static void Insert<Type>(SqlMapper sqlm, Type record) where Type : ISqlUserRecord {
            String[] headers = sqlm.SqlDatabasePattern[record.GetTableLabel()].Headers;

            Dictionary<String, String> values = new Dictionary<String, String>();
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(record);

            foreach (String header in headers) {
                PropertyDescriptor descriptor = pdc.Find(header, false);
                if (descriptor != null) {
                    Object value = descriptor.GetValue(record);
                    if (value != null || sqlm.SqlDatabasePattern[record.GetTableLabel()][header].AllowNull) {
                        values[descriptor.Name] = value as String ?? "Null";
                    } else if (sqlm.SqlDatabasePattern[record.GetTableLabel()][header].Default != null) {
                        values[descriptor.Name] = sqlm.SqlDatabasePattern[record.GetTableLabel()][header].Default as String;
                    }

                    values[descriptor.Name] = String.Format("'{0}'", descriptor.GetValue(record));
                }
            }

            ISqlRequest sqlr = new SqlNonQuerryRequest {
                Request = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", record.GetTableLabel(), String.Join(",", values.Keys.ToArray()), String.Join(",", values.Values.ToArray()))
            };

            SqlConnector.Execute(sqlm.SqlConnector, ref sqlr);
        }

        public static void Insert(SqlMapper sqlm, SqlDynamicRecord record, String table) {
            String[] headers = sqlm.SqlDatabasePattern[table].Headers;

            Dictionary<String, String> values = new Dictionary<String, String>();

            foreach (String header in headers) {
                if(record.HasProperty(header)) {
                    values[header] = String.Format("'{0}'", record[header]);
                }
            }

            ISqlRequest sqlr = new SqlNonQuerryRequest {
                Request = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", table, String.Join(",", values.Keys.ToArray()), String.Join(",", values.Values.ToArray()))
            };

            SqlConnector.Execute(sqlm.SqlConnector, ref sqlr);
        }

        public static String GetPattern(SqlMapper sqlm) {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<String, SqlTablePattern> tp in sqlm.SqlDatabasePattern) {
                sb.AppendFormat("[{0,-5}{1,-30}{2,-10}{3,-10}{4,-20}{5,-10}] {6}\n", "ORD", "NAME", "TYPE", "NULL", "PRIMARY", "FOREING", tp.Key);
                foreach (KeyValuePair<String, SqlColumnData> cp in tp.Value) {
                    SqlColumnData cd = cp.Value;
                    sb.AppendFormat("[{0,-5}{1,-30}{2,-10}{3,-10}{4,-20}{5,-10}]\n", cd.Ordinal, cd.Label, cd.Type, cd.AllowNull, cd.PrimaryKey, cd.ForeignKey);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}