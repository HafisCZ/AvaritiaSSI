using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public class SqlConnectionString
    {
        private StringBuilder sb;

        public SqlConnectionString(String server, String database)
        {
            sb = new StringBuilder().AppendFormat("Data Source={0};Initial Catalog={1};", server, database);
        }

        public SqlConnectionString Login(String username, String password)
        {
            sb.AppendFormat("User ID={0};Password={1};", username, password);
            return this;
        }

        public SqlConnectionString SetTrusted(Boolean trusted)
        {
            sb.AppendFormat("Trusted_Connection={0};", trusted ? "True" : "False");
            return this;
        }

        public void Reset()
        {
            sb.Clear();
        }

        public override String ToString() => sb.ToString();
    }
}
