using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel;

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
        public String Request { get; set; }
        public int RowsAffected { get; set; }
        public List<object[]> Response { get; internal set; }
    }

    public class SQLSyncRequest
    {
        public List<ISQLRecord> Records { get; set; }
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

        public void Sync(SQLSyncRequest request)
        {
            if (SQLConnection.State != ConnectionState.Open) {
                throw new Exception("SQLNotConnectedException");
            }

            foreach (ISQLRecord isqlr in request.Records) {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("UPDATE {0} SET ", isqlr.SourceTable);

                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(isqlr);
                String[] headers = isqlr.Mapper.VirtualMap[isqlr.SourceTable].Headers;

                // TODO

                sb.AppendFormat("WHERE 1=1");
                Console.WriteLine(sb.ToString());

                using (SqlCommand sqlc = new SqlCommand(sb.ToString(), SQLConnection)) {
                    sqlc.ExecuteNonQuery();
                }
            }
        }

        public void Fetch(ref SQLRequest request, CommandBehavior behaviour = CommandBehavior.SequentialAccess) {
            if (SQLConnection.State != ConnectionState.Open) {
                throw new Exception("SQLNotConnectedException");
            }

            using (SqlCommand sqlc = new SqlCommand(request.Request, SQLConnection))
            using (SqlDataReader sqldr = sqlc.ExecuteReader(behaviour)) {
                request.Response = new List<object[]>();
                request.RowsAffected = sqldr.RecordsAffected;

                while (sqldr.Read()) {
                    object[] record = new object[sqldr.FieldCount];
                    sqldr.GetValues(record);
                    request.Response.Add(record);
                }
            }
        }
    }
}