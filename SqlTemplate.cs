using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    internal sealed class SqlColumnTemplate
    {
        public String Label { get; }
        public Int32 Ordinal { get; }
        public Boolean AllowsNull { get; }
        public Boolean IsPrimary { get; private set; }
        public Boolean HasDefault { get; private set; }

        public SqlColumnTemplate(String label, Int32 ordinal, Boolean primaryKey, Boolean allowNull, Boolean hasDefault)
        {
            Label = label;
            Ordinal = ordinal;
            IsPrimary = primaryKey;
            AllowsNull = allowNull;
            HasDefault = hasDefault;
        }

        public void SetPrimary(Boolean isPrimary) => IsPrimary = isPrimary;
        public void SetDefault(Boolean hasDefault) => HasDefault = hasDefault;
    }

    internal sealed class SqlTableTemplate
    {
        public String Label { get; }
        public Dictionary<String, SqlColumnTemplate> Columns { get; } = new Dictionary<String, SqlColumnTemplate>();

        public SqlTableTemplate(String label)
        {
            Label = label;
        }

        public void Add(SqlColumnTemplate column) => Columns.Add(column.Label, column);
    }

    internal sealed class SqlRoutineTemplate
    {
        public String Label { get; }
        public Boolean IsFunction { get; }
        public Boolean IsProcedure { get; }
        public Dictionary<Int32, Boolean> Params { get; }

        public SqlRoutineTemplate(String label, Boolean isProcedure)
        {
            Label = label;
            IsProcedure = isProcedure;
            IsFunction = !isProcedure;

            Params = new Dictionary<Int32, Boolean>();
        }
    }
}
