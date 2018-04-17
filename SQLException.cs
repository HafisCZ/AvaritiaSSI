using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaritia
{
    public class SQLNotConnectedException : Exception
    {
        public SQLNotConnectedException() : base() { }
        public SQLNotConnectedException(string msg) : base(msg) { }
        public SQLNotConnectedException(string msg, Exception e) : base(msg, e) { }
    }

    public class SQLCannotCreateVirtualMapException : Exception
    {
        public SQLCannotCreateVirtualMapException() : base() { }
        public SQLCannotCreateVirtualMapException(string msg) : base(msg) { }
        public SQLCannotCreateVirtualMapException(string msg, Exception e) : base(msg, e) { }
    }

    public class SQLVirtualMapFetchException : Exception
    {
        public SQLVirtualMapFetchException() : base() { }
        public SQLVirtualMapFetchException(string msg) : base(msg) { }
        public SQLVirtualMapFetchException(string msg, Exception e) : base(msg, e) { }
    }

    public class SQLResourceUnavailable : Exception
    {
        public SQLResourceUnavailable() : base() { }
        public SQLResourceUnavailable(string msg) : base(msg) { }
        public SQLResourceUnavailable(string msg, Exception e) : base(msg, e) { }
    }

    public class SQLInvalidQuerry : Exception
    {
        public SQLInvalidQuerry() : base() { }
        public SQLInvalidQuerry(string msg) : base(msg) { }
        public SQLInvalidQuerry(string msg, Exception e) : base(msg, e) { }
    }
}
