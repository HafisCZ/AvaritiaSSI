using System;
using System.Collections.Generic;
using System.Data;
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
    }

    public class SqlQuerryRequest : SqlNonQuerryRequest
    {
        public List<Object[]> Response { get; set; } = new List<Object[]>();
    }
}
