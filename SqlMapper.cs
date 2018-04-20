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
        internal SqlConnector Connector { get; private set; }
        internal SqlDatabaseTemplate Template { get; private set; }

        public SqlMapper(SqlConnector connector, String tablePrefix = null) {
            Connector = connector;
            Template = GetTemplate(connector, tablePrefix);
        }

        internal static SqlDatabaseTemplate GetTemplate(SqlConnector connector, String tablePrefix)
        {
            SqlDatabaseTemplate database = new SqlDatabaseTemplate();

            ISqlRequest request = new SqlQuerryRequest { Request = String.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'{0}", tablePrefix != null ? String.Format(" AND TABLE_NAME LIKE '{0}%'", tablePrefix) : "") };
            SqlConnector.Execute(connector, ref request);
            foreach (Object[] row in (request as SqlQuerryRequest).Response) {
                database[row[0] as String] = new SqlTableTemplate { Label = row[0] as String };
            }

            request = new SqlQuerryRequest { Request = String.Format("SELECT TABLE_NAME, COLUMN_NAME, ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS{0}", tablePrefix != null ? String.Format(" WHERE TABLE_NAME LIKE '{0}%'", tablePrefix) : "") };
            SqlConnector.Execute(connector, ref request);
            foreach (Object[] row in (request as SqlQuerryRequest).Response) {
                database[row[0] as String][row[1] as String] = new SqlColumnData {
                    Label = row[1] as String,
                    Ordinal = (row[2] as Int32? ?? 0) - 1,
                    Default = row[3],
                    AllowNull = row[4] as Boolean? ?? false,
                    Type = row[5] as String,
                    Length = row[6] as Int32?
                };
            }

            request = new SqlQuerryRequest { Request = "SELECT TC.TABLE_NAME, CCU.COLUMN_NAME, TC.CONSTRAINT_TYPE FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC LEFT JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE CCU ON TC.CONSTRAINT_NAME = CCU.CONSTRAINT_NAME" };
            SqlConnector.Execute(connector, ref request);
            foreach (Object[] row in (request as SqlQuerryRequest).Response) {
                String tempTable = row[0] as String;
                String tempColumn = row[1] as String;
                String tempConstraint = row[2] as String;

                if (database.ContainsKey(tempTable)) {
                    SqlColumnData col = database[tempTable][tempColumn];
                    col.PrimaryKey = tempConstraint == "PRIMARY KEY";
                    col.ForeignKey = tempConstraint == "FOREIGN KEY";
                }
            }

            SqlDatabaseTemplate.MakeTables(ref database);
            return database;
        }

        public static void Select<Type>(SqlMapper mapper, out List<Type> list, String table, String filter, params String[] headers) where Type : ISqlUserRecord, new() {
            String[] requestedHeaders = headers.Length > 0 ? headers : mapper.Template[table].Headers;
            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(typeof(Type));
            list = new List<Type>();

            SqlQuerryRequest request = new SqlQuerryRequest { Request = String.Format("SELECT {0} FROM {1}{2}", String.Join(", ", headers), table, (filter == null) ? "" : String.Format(" WHERE {0}", filter)) };
            SqlConnector.ExecuteQuerry(mapper.Connector, ref request, System.Data.CommandBehavior.SequentialAccess);
            foreach (Object[] record in request.Response) {
                Type userRecord = new Type();
                for (Int32 ord = 0; ord < record.Length; ord++) {
                    propertyDescriptors.Find(requestedHeaders[ord], false)?.SetValue(userRecord, record[ord]);
                }

                list.Add(userRecord);
            }
        }

        public static void Update<Type>(SqlMapper mapper, ref Type record) where Type : ISqlUserRecord {
            Dictionary<String, Boolean> headers = new Dictionary<String, Boolean>();
            foreach (SqlColumnData columnData in mapper.Template[record.GetTableLabel()].Values) {
                headers[columnData.Label] = columnData.PrimaryKey;
            }

            Dictionary<String, Object> vals = new Dictionary<String, Object>();
            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(record);
            foreach (KeyValuePair<String, Boolean> pair in headers) {
                PropertyDescriptor descriptor = propertyDescriptors.Find(pair.Key, false);
                if (descriptor == null && pair.Value) {
                    throw new Exception();
                } else if (descriptor != null) {
                    vals[pair.Key] = descriptor.GetValue(record);
                }
            }

            String primary = String.Join(",", headers.Where(pair => pair.Value).Select(pair => String.Format("{0}='{1}'", pair.Key, vals[pair.Key])));
            String values = String.Join(",", headers.Where(pair => !pair.Value).Select(pair => String.Format("{0}='{1}'", pair.Key, vals[pair.Key])));

            SqlNonQuerryRequest request = new SqlNonQuerryRequest {
                Request = String.Format("UPDATE {0} SET {1} WHERE {2}", record.GetTableLabel(), primary, values)
            };
            SqlConnector.ExecuteNonQuerry(mapper.Connector, ref request);
        }

        public static void Insert<Type>(SqlMapper mapper, Type record) where Type : ISqlUserRecord {
            Dictionary<String, String> values = new Dictionary<String, String>();
            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(record);
            foreach (SqlColumnData columnData in mapper.Template[record.GetTableLabel()].Values) {
                PropertyDescriptor descriptor = propertyDescriptors.Find(columnData.Label, false);
                if (descriptor == null && !columnData.AllowNull) {
                    throw new Exception();
                } else if (descriptor != null) {
                    values[columnData.Label] = String.Format("{0}", descriptor.GetValue(record));
                }
            }

            SqlNonQuerryRequest request = new SqlNonQuerryRequest {
                Request = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", record.GetTableLabel(), String.Join(",", values.Keys), String.Join(",", values))
            };
            SqlConnector.ExecuteNonQuerry(mapper.Connector, ref request);
        }

        [Obsolete]
        public static void Select(SqlMapper sqlm, out List<SqlDynamicRecord> list, String table, params String[] cols) {
            String[] header = cols.Length > 0 ? cols : sqlm.Template[table].Headers;
            SqlQuerryRequest sqlqr = new SqlQuerryRequest {
                Request = String.Format("SELECT {0} FROM {1}", String.Join(", ", header), table)
            };
            SqlConnector.ExecuteQuerry(sqlm.Connector, ref sqlqr, System.Data.CommandBehavior.SequentialAccess);

            list = new List<SqlDynamicRecord>();

            foreach (Object[] row in (sqlqr as SqlQuerryRequest).Response) {
                SqlDynamicRecord dr = new SqlDynamicRecord(table);

                for (Int32 ordinal = 0; ordinal < row.Length; ordinal++) {
                    dr[header[ordinal]] = row[ordinal];
                }

                list.Add(dr);
            }
        }

        [Obsolete]
        public static void Update(SqlMapper sqlm, ref SqlDynamicRecord record) {
            List<String> keys = new List<String>();
            List<String> cols = new List<String>();
            foreach (SqlColumnData data in sqlm.Template[record.TableName].Values.ToArray()) {
                (data.PrimaryKey ? keys : cols).Add(data.Label);
            }

            StringBuilder keyBuilder = new StringBuilder();
            for (Int32 i = 0; i < keys.Count; i++) {
                keyBuilder.AppendFormat("{0}{1}='{2}'", (i > 0 ? "," : " "), keys[i], record[keys[i]]);
            }

            StringBuilder colBuilder = new StringBuilder();
            for (Int32 i = 0; i < cols.Count; i++) {
                if (record.HasProperty(cols[i])) {
                    colBuilder.AppendFormat("{0}{1}='{2}'", (i > 0 ? "," : " "), cols[i], record[cols[i]]);
                }
            }

            SqlNonQuerryRequest request = new SqlNonQuerryRequest { Request = String.Format("UPDATE {0} SET {1} WHERE {2}", record.TableName, colBuilder.ToString(), keyBuilder.ToString()) };
            SqlConnector.ExecuteNonQuerry(sqlm.Connector, ref request);
        }

        [Obsolete]
        public static void Insert(SqlMapper sqlm, SqlDynamicRecord record, String table)
        {
            String[] headers = sqlm.Template[table].Headers;

            Dictionary<String, String> values = new Dictionary<String, String>();

            foreach (String header in headers) {
                if (record.HasProperty(header)) {
                    values[header] = String.Format("'{0}'", record[header]);
                }
            }

            ISqlRequest sqlr = new SqlNonQuerryRequest {
                Request = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", table, String.Join(",", values.Keys.ToArray()), String.Join(",", values.Values.ToArray()))
            };

            SqlConnector.Execute(sqlm.Connector, ref sqlr);
        }

        [Obsolete]
        public static String GetPattern(SqlMapper sqlm) {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<String, SqlTableTemplate> tp in sqlm.Template) {
                sb.AppendFormat("[{0,-5}{1,-30}{2,-10}{3,-10}{4,-20}{5,-10}] {6}\n", "ORD", "NAME", "TYPE", "NULL", "PRIMARY", "FOREING", tp.Key);
                foreach (KeyValuePair<String, SqlColumnData> cp in tp.Value) {
                    SqlColumnData cd = cp.Value;
                    sb.AppendFormat("[{0,-5}{1,-30}{2,-10}{3,-10}{4,-20}{5,-10}]\n", cd.Ordinal, cd.Label, cd.Type, cd.AllowNull, cd.PrimaryKey, cd.ForeignKey);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        [Obsolete]
        public static String GetDebug(SqlMapper sqlm) {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<String, SqlTableTemplate> tp in sqlm.Template) {
                sb.AppendFormat("  - {0}\n", tp.Key);
                foreach (KeyValuePair<String, SqlColumnData> cp in tp.Value) {
                    sb.AppendFormat("    - {0}\n", cp.Key);
                }
            }

            return sb.ToString();
        }
    }
}