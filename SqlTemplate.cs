﻿using System;
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
        public Boolean PrimaryKey { get; private set; }
        public Boolean AllowNull { get; }
        public Boolean HasDefault { get; }

        [Obsolete("Unused")]
        public String DbType { get; }
        [Obsolete("Unused")]
        public Int32? Length { get; }

        public SqlColumnTemplate(String label, Int32 ordinal, Boolean primaryKey, Boolean allowNull, Boolean hasDefault)
        {
            Label = label;
            Ordinal = ordinal;
            PrimaryKey = primaryKey;
            AllowNull = allowNull;
            HasDefault = hasDefault;
        }

        public void MarkAsPrimary()
        {
            PrimaryKey = true;
        }
    }

    internal sealed class SqlTableTemplate
    {
        public Dictionary<String, SqlColumnTemplate> Columns { get; }
        public String Label { get; }

        public SqlTableTemplate(String label)
        {
            Label = label;
            Columns = new Dictionary<String, SqlColumnTemplate>();
        }

        public void AddColumn(SqlColumnTemplate column)
        {
            Columns.Add(column.Label, column);
        }

        public SqlColumnTemplate this[String column]
        {
            get {
                return Columns[column];
            }
        }
    }
}
