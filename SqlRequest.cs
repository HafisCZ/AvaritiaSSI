using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public interface ISqlRequest
    {
        String Request { get; set; }
        Int32 RowsAffected { get; }
    }

    public class SqlNonQuerryRequest : ISqlRequest
    {
        public String Request { get; set; }
        public Int32 RowsAffected { get; internal set; }

        public static SqlQuerryRequest MakeTableConstraintRequest() => new SqlQuerryRequest {
            Request = "SELECT TC.TABLE_NAME, CCU.COLUMN_NAME, TC.CONSTRAINT_TYPE FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC LEFT JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE CCU ON TC.CONSTRAINT_NAME = CCU.CONSTRAINT_NAME"
        };

        public static SqlQuerryRequest MakeTableDefinitionRequest(String prefix) => new SqlQuerryRequest {
            Request = String.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'{0}", prefix != null ? String.Format(" AND TABLE_NAME LIKE '{0}%'", prefix) : "")
        };

        public static SqlQuerryRequest MakeColumnDefinitionRequest(String prefix) => new SqlQuerryRequest {
            Request = String.Format("SELECT TABLE_NAME, COLUMN_NAME, ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS{0}", prefix != null ? String.Format(" WHERE TABLE_NAME LIKE '{0}%'", prefix) : "")
        };

        public static SqlQuerryRequest MakeSelectRequest(String table, String[] cols, String where = null) => new SqlQuerryRequest {
            Request = String.Format("SELECT {0} FROM {1}{2}", String.Join(", ", cols), table, (where == null) ? "" : String.Format(" WHERE {0}", where))
        };
    }

    public class SqlQuerryRequest : SqlNonQuerryRequest
    {
        public List<Object[]> Response { get; set; } = new List<Object[]>();
    }
}
