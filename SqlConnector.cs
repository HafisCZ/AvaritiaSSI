using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Avaritia
{
    public class SqlConnectionString
    {
        private StringBuilder sb;

        public SqlConnectionString(String server, String database) {
            sb = new StringBuilder().AppendFormat("Data Source={0};Initial Catalog={1};", server, database);
        }

        public SqlConnectionString Login(String username, String password) {
            sb.AppendFormat("User ID={0};Password={1};", username, password);
            return this;
        }

        public SqlConnectionString SetTrusted(Boolean trusted) {
            sb.AppendFormat("Trusted_Connection={0};", trusted ? "True" : "False");
            return this;
        }

        public void Reset() {
            sb.Clear();
        }

        public override String ToString() => sb.ToString();
    }

    public class SqlConnector : IDisposable
    {
        internal SqlConnection SqlConnection { get; private set; }

        public SqlConnector(SqlConnectionString sqlcs) {
            SqlConnection = new SqlConnection(sqlcs.ToString());
        }

        public void Dispose() {
            SqlConnection.Dispose();
        }

        public SqlConnector Open() {
            SqlConnection.Open();
            return this;
        }

        public SqlConnector Close() {
            SqlConnection.Close();
            return this;
        }

        internal static void Execute(SqlConnector sqlc, ref ISqlRequest sqlr, CommandBehavior cb) {
            using (SqlCommand sqlcmd = new SqlCommand(sqlr.Request, sqlc.SqlConnection)) {
                if (sqlr is SqlQuerryRequest) {
                    SqlQuerryRequest sqlqr = sqlr as SqlQuerryRequest;
                    using (SqlDataReader sqldr = sqlcmd.ExecuteReader(cb)) {
                        sqlqr.RowsAffected = sqldr.RecordsAffected;
                        while (sqldr.Read()) {
                            Object[] objr = new Object[sqldr.FieldCount];
                            sqldr.GetValues(objr);
                            sqlqr.Response.Add(objr);
                        }
                    }
                } else {
                    SqlNonQuerryRequest sqlnqr = sqlr as SqlNonQuerryRequest;
                    sqlnqr.RowsAffected = sqlcmd.ExecuteNonQuery();
                }
            }
        }

        public static void Execute(SqlConnector sqlc, ref ISqlRequest sqlr) => Execute(sqlc, ref sqlr, CommandBehavior.SequentialAccess);
    }   
}