using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace pind_server_sqlite.Common
{
    public class LoginException : Exception
    {
        public LoginException(string msg) : base(msg) { }
    }
}