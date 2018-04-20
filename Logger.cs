using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public static class Logger
    {
        public const String TRANSACTION_ROLLBACK = "FAILED";
        public const String TRANSACTION_COMMIT = "SUCCESS";
        public const String TRANSACTION_START = "TRANSACTION START";
        public const String CONNECTION_OPEN = "CONN OPEN";
        public const String CONNECTION_CLOSE = "CONN CLOSE";
        public const String TEMPLATE_DOES_NOT_EXIST = "TEMPL DNE";
        public const String TEMPLATE_CREATE = "TEMPL CREATE";
        public const String TEMPLATE_FINISHED = "TEMPL FINISH";
        public const String TABLE_GET = "TABLE GET";

        public static Boolean Enable { get; set; }

        public static List<Tuple<Int64, String>> Logs { get; } = new List<Tuple<Int64, String>>();

        public static void Clear()
        {
            Logs.Clear();
        }

        public static void Log(String message)
        {
            if (Enable) {
                Logs.Add(new Tuple<Int64, String>(DateTime.Now.Ticks, message));
            }
        }
    }

}
