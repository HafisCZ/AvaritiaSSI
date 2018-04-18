using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Avaritia
{
    public class SQLConnectionString
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string TrustedConnection { get; set; }

        public string String {
            get {
                return String.Format("Server={0};Database={1};Trusted_Connection={2}", Server, Database, TrustedConnection);
            }
        }
    }

    public class SQLRequest
    {
        public string Request { get; set; }
        public List<object[]> Response { get; set; }
    }

    public class SQLHook : IDisposable
    {
        internal SqlConnection SQLConnection { get; set; }

        public SQLHook(SQLConnectionString sqlcs) {
            SQLConnection = new SqlConnection(sqlcs.String);
            SQLConnection.Open();

            if (SQLConnection.State != ConnectionState.Open) {
                throw new Exception("SQLNotConnectedException");
            }
        }

        public void Dispose() {
            SQLConnection.Dispose();
        }

        public void Fetch(ref SQLRequest request, CommandBehavior behaviour = CommandBehavior.SequentialAccess) {
            if (SQLConnection.State != ConnectionState.Open) {
                throw new Exception("SQLNotConnectedException");
            }

            using (SqlCommand sqlc = new SqlCommand(request.Request, SQLConnection))
            using (SqlDataReader sqldr = sqlc.ExecuteReader(behaviour)) {
                request.Response = new List<object[]>();
                while (sqldr.Read()) {
                    object[] record = new object[sqldr.FieldCount];
                    sqldr.GetValues(record);
                    request.Response.Add(record);
                }
            }
        }
    }
}