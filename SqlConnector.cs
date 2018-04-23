using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public sealed class SqlConnector : IDisposable
    {
        private SqlConnection Connection { get; }
        private SqlTransaction Transaction { get; set; }
        private Boolean Frozen { get; set; } = false;

        internal SqlConnector(String connectionString)
        {
            Connection = new SqlConnection(connectionString);
            Connection.Open();

            Logger.Log(Logger.Action.CONNECTION_OPEN, connectionString);
        }

        public void Dispose()
        {
            Connection.Dispose();

            Logger.Log(Logger.Action.CONNECTION_CLOSE);
        }

        public void TransactionPackBegin()
        {
            Frozen = true;
        }

        public void TransactionPackEnd()
        {
            Frozen = false;
            try {
                Transaction?.Commit();
                Logger.Log(Logger.Action.TRANSACTION_END_SUCCESS);
                Transaction = null;
            } catch (SqlException) {
                Transaction?.Rollback();
                Logger.Log(Logger.Action.TRANSACTION_END_FAIL);
            } finally {
                Transaction = null;
            }
        }

        internal void SetTransaction(IsolationLevel isolationLevel)
        {
            if (Transaction == null) {
                Transaction = Connection.BeginTransaction(isolationLevel);
                Logger.Log(Logger.Action.TRANSACTION_BEGIN, isolationLevel.ToString());
            }
        }

        internal Boolean Execute(ref SqlRequest request, Boolean useTransaction)
        {
            try {
                if (useTransaction) {
                    SetTransaction(IsolationLevel.Serializable);
                }

                Logger.Log(Logger.Action.EXECUTE_QUERRY, request.Statement);

                using (SqlCommand command = new SqlCommand(request.Statement, Connection, Transaction))
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess)) {
                    request.Headers = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                    request.Response = new List<Object[]>();
                    while (reader.Read()) {
                        Object[] data = new Object[reader.FieldCount];
                        reader.GetValues(data);
                        request.Response.Add(data);
                    }
                }

                if (!Frozen) {
                    Transaction?.Commit();
                    Logger.Log(Logger.Action.TRANSACTION_END_SUCCESS);
                }

                return true;
            } catch (SqlException) {
                Transaction?.Rollback();
                Logger.Log(Logger.Action.TRANSACTION_END_FAIL);
            } finally {
                if (!Frozen) {
                    Transaction = null;
                }
            }

            return false;
        }

        internal Boolean ExecuteNonQuerry(ref SqlRequest request, Boolean useTransaction)
        {
            try {
                if (useTransaction) {
                    SetTransaction(IsolationLevel.Serializable);
                }

                Logger.Log(Logger.Action.EXECUTE_NONQUERRY, request.Statement);

                using (SqlCommand command = new SqlCommand(request.Statement, Connection, Transaction)) {
                    request.RecordsAffected = command.ExecuteNonQuery();
                }

                if (!Frozen) {
                    Transaction?.Commit();
                    Logger.Log(Logger.Action.TRANSACTION_END_SUCCESS);
                }

                return true;
            } catch (SqlException e) {
                Transaction?.Rollback();
                Logger.Log(Logger.Action.TRANSACTION_END_FAIL);
            } finally {
                if (!Frozen) {
                    Transaction = null;
                }
            }

            return false;
        }
    }

    public sealed class SqlRequest
    {
        public String Statement { get; set; }

        public Int32 RecordsAffected { get; set; }

        public List<Object[]> Response { get; set; }
        public List<String> Headers { get; set; }
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
