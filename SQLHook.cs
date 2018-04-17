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

        public string String
        {
            get {
                StringBuilder builder = new StringBuilder();

                if (Server != null) builder.AppendFormat("Server={0};", Server);
                if (Database != null) builder.AppendFormat("Database={0};", Database);
                if (TrustedConnection != null) builder.AppendFormat("Trusted_Connection={0};", TrustedConnection);

                return builder.ToString();
            }
        }
    }

    public class SQLRequest
    {
        public List<object[]> Response { get; set; } = null;
        public string Request { get; set; } = null;
    }

    public class SQLHook : IDisposable
    {
        private SqlConnection SQLConnection { get; set; }

        public SQLHook(SQLConnectionString sqlcs)
        {
            SQLConnection = new SqlConnection(sqlcs.String);
            SQLConnection.Open();

            if (SQLConnection.State != ConnectionState.Open) {
                throw new SQLNotConnectedException();
            }
        }

        public void Dispose()
        {
            SQLConnection.Dispose();
        }

        public void Read(ref SQLRequest request, CommandBehavior behaviour = CommandBehavior.SequentialAccess)
        {
            if (SQLConnection.State != ConnectionState.Open) {
                throw new SQLNotConnectedException();
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