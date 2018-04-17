using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public interface IMappedType
    {
        dynamic Get();
    }

    public class SQLInteger : IMappedType
    {
        private int value;

        public SQLInteger(int value)
        {
            this.value = value;
        }

        public dynamic Get()
        {
            return value;
        }
    }

    public class SQLString : IMappedType
    {
        private string value;

        public SQLString(string value)
        {
            this.value = value;
        }

        public dynamic Get()
        {
            return value;
        }
    }

    public class SQLDate : IMappedType
    {
        private DateTime value;

        public SQLDate(DateTime value)
        {
            this.value = value;
        }

        public dynamic Get()
        {
            return value;
        }
    }

    public class Record
    {
        public IList<IMappedType> Values { get; } = new List<IMappedType>();
        public int Count { get { return Values.Count; } }
    }

}
