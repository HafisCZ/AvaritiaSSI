using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Avaritia
{
    public sealed class SqlConnector : IDisposable
    {
        private SqlConnection Connection { get; }

        internal SqlConnector(String connectionString)
        {
            try {
                Connection = new SqlConnection(connectionString);
                Connection.Open();

                Logger.Log(Logger.Action.CONNECTION_OPEN, connectionString);
            } catch (SqlException e) {
                Logger.Log(Logger.Action.CONNECTION_FAIL, e.Message);
            }
        }

        ~SqlConnector()
        {
            Dispose();
        }

        public void Dispose()
        {
            Connection.Dispose();

            Logger.Log(Logger.Action.CONNECTION_CLOSE);
        }

        internal Boolean Execute(ref SqlRequest request, Boolean nonQuerry = false)
        {
            try {
                using (TransactionScope transactionScope = new TransactionScope()) {
                    Logger.Log(Logger.Action.TRANSACTION_BEGIN, Transaction.Current.IsolationLevel.ToString());

                    using (SqlCommand command = new SqlCommand(request.Statement, Connection)) {
                        if (nonQuerry) {
                            Logger.Log(Logger.Action.EXECUTE_NONQUERRY, request.Statement);

                            request.RecordsAffected = command.ExecuteNonQuery();
                        } else {
                            Logger.Log(Logger.Action.EXECUTE_QUERRY, request.Statement);

                            using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess)) {
                                request.Headers = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                                request.Response = new List<Object[]>();
                                while (reader.Read()) {
                                    Object[] data = new Object[reader.FieldCount];
                                    reader.GetValues(data);
                                    request.Response.Add(data);
                                }
                            }
                        }
                    }

                    transactionScope.Complete();

                    Logger.Log(Logger.Action.TRANSACTION_END_SUCCESS);
                }

                return true;
            } catch (TransactionAbortedException e) {
                Logger.Log(Logger.Action.TRANSACTION_END_FAIL, e.Message);

                return false;
            } catch (ApplicationException e) {
                throw e;
            }
        }
    }

    public sealed class SqlRequest
    {
        public String Statement { get; set; }
        public Int32 RecordsAffected { get; internal set; }
        public List<Object[]> Response { get; internal set; }
        public List<String> Headers { get; internal set; }
    }

    public sealed class SqlConnectionStringBuilder
    {
        private StringBuilder builder = new StringBuilder();

        public SqlConnectionStringBuilder(String server, String database)
        {
            builder.AppendFormat("Data Source={0};Initial Catalog={1};", server, database);
        }

        public SqlConnectionStringBuilder Login(String username, String password)
        {
            builder.AppendFormat("User ID={0};Password={1};", username, password);
            return this;
        }

        public SqlConnectionStringBuilder SetTrusted(Boolean trusted)
        {
            builder.AppendFormat("Trusted_Connection={0};", trusted ? "True" : "False");
            return this;
        }

        public override String ToString()
        {
            return builder.ToString();
        }
    }
}
