using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public static class Logger
    {
        public enum Action
        {
            UNDEFINED,
            TRANSACTION_BEGIN,
            TRANSACTION_END_SUCCESS,
            TRANSACTION_END_FAIL,
            CONNECTION_OPEN,
            CONNECTION_CLOSE,
            TEMPLATE_NOTFOUND,
            TEMPLATE_MAP_BEGIN,
            TEMPLATE_MAP_END,
            TYPECACHE_HIT,
            TYPECACHE_MAP,
            EXECUTE_QUERRY,
            EXECUTE_NONQUERRY,
            TEMPLATE_ROUTINE_MAP_BEGIN,
            TEMPLATE_ROUTINE_MAP_END
        }

        public static List<Tuple<Int64, Action, String>> Logs { get; } = new List<Tuple<Int64, Action, String>>();
        public static Boolean EnableLogger { get; set; }

        public static void Log(Action action = Action.UNDEFINED, String message = null)
        {
            if (EnableLogger) {
                Logs.Add(new Tuple<Int64, Action, String>(DateTime.Now.Ticks, action, message));
            }
        }
    }

}
