using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public static class Logger
    {
        public const String TRANSACTION_ROLLBACK = "[TRANSACTION][ROLLBACK] ";
        public const String TRANSACTION_COMMIT = "[TRANSACTION][SUCCESS] ";
        public const String TRANSACTION_START = "[TRANSACTION][BEGIN] ";
        public const String CONNECTION_OPEN = "[CONNECTION][OPEN] ";
        public const String CONNECTION_CLOSE = "[CONNECTION][CLOSE] ";
        public const String TEMPLATE_DOES_NOT_EXIST = "[TEMPLATE][NOT_EXISTS_ERROR] ";
        public const String TEMPLATE_CREATE = "[TEMPLATE][BEGIN_MAP] ";
        public const String TEMPLATE_FINISHED = "[TEMPLATE][END_MAP] ";
        public const String TYPECACHE_INSERT = "[TYPECACHE][INSERT] ";
        public const String QUERRY = "[QUERRY] ";

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
