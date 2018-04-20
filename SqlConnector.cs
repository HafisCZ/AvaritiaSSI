using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Avaritia
{
    public class SqlConnector : IDisposable
    {
        public Boolean Debug { get; set; }
        internal SqlConnection Connection { get; private set; }
        internal SqlTransaction Transaction { get; private set; }

        public SqlConnector(SqlConnectionString connectionString, Boolean debug = false) {
            Connection = new SqlConnection(connectionString.ToString());
            Debug = debug;
        }

        public void Dispose() => Connection.Dispose();
        public void Open() => Connection.Open();
        public void Close() => Connection.Close();

        public void SetTransaction(IsolationLevel level) => Transaction = Connection.BeginTransaction(level);

        internal void Log(String message) {
            if (Debug) {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(String.Format("[{0}] {1}", DateTime.Now.Ticks, message));
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        internal static void ExecuteQuerry(SqlConnector sqlc, ref SqlQuerryRequest sqlr, CommandBehavior cb) {
            sqlc.Log(sqlr.Request);
            try {
                using (SqlCommand sqlcmd = new SqlCommand(sqlr.Request, sqlc.Connection, sqlc.Transaction))
                using (SqlDataReader reader = sqlcmd.ExecuteReader(cb)) {
                    sqlr.RowsAffected = reader.RecordsAffected;
                    while (reader.Read()) {
                        Object[] objr = new Object[reader.FieldCount];
                        reader.GetValues(objr);
                        sqlr.Response.Add(objr);
                    }
                }

                sqlc.Transaction?.Commit();
                sqlc.Log("REQUEST SUCCEEDED");
            } catch (SqlException) {
                sqlc.Transaction?.Rollback();
                sqlc.Log("REQUEST FAILED");
            } finally {
                sqlc.Transaction = null;
            }
        }

        internal static void ExecuteNonQuerry(SqlConnector sqlc, ref SqlNonQuerryRequest sqlr) {
            sqlc.Log(sqlr.Request);
            try {
                using (SqlCommand sqlcmd = new SqlCommand(sqlr.Request, sqlc.Connection, sqlc.Transaction)) {
                    sqlr.RowsAffected = sqlcmd.ExecuteNonQuery();
                }

                sqlc.Transaction?.Commit();
                sqlc.Log("REQUEST SUCCEEDED");
            } catch (SqlException) {
                sqlc.Transaction?.Rollback();
                sqlc.Log("REQUEST FAILED");
            } finally {
                sqlc.Transaction = null;
            }
        }
        
        public static void Execute(SqlConnector sqlc, ref ISqlRequest sqlr) {
            if (sqlr is SqlQuerryRequest) {
                SqlQuerryRequest request = sqlr as SqlQuerryRequest;
                ExecuteQuerry(sqlc, ref request, CommandBehavior.SequentialAccess);
            } else {
                SqlNonQuerryRequest request = sqlr as SqlNonQuerryRequest;
                ExecuteNonQuerry(sqlc, ref request);
            }
        }
    }   
}