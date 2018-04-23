using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public sealed class SqlRoutine
    {
        private SqlRoutineTemplate Template { get; }
        private SqlConnector Connector { get; }

        public String Label { get; }

        internal SqlRoutine(SqlConnector connector, SqlRoutineTemplate template)
        {
            Connector = connector;
            Label = template.Label;
            Template = template;
        }

        public Boolean Execute(out SqlRequest request, params Object[] parameters)
        {
            request = new SqlRequest {
                Statement = Template.IsFunction ? 
                String.Format("SELECT * FROM {0}({1})", Label, String.Join(",", parameters.Select(p => Parse(p)))) : 
                String.Format("EXECUTE {0} {1}", Label, String.Join(" ", parameters.Select(p => Parse(p))))
            };

            return Template.IsProcedure ? Connector.ExecuteNonQuerry(ref request, false) : Connector.Execute(ref request, false);
        }

        internal String Parse(Object o)
        {
            if (o.GetType() == typeof(DateTime)) {
                return String.Format("'{0}'", (o as DateTime?)?.Date.ToString("MM-dd-yyyy"));
            } else if (o.GetType() == typeof(String)) {
                if ((o as String).Equals("default")) {
                    return o as String;
                } else {
                    return String.Format("'{0}'", (o as String).Replace("'", "''"));
                }
            } else {
                return String.Format("'{0}'", o);
            }
        }
    }
}
