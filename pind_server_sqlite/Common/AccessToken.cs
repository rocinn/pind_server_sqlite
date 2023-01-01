using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace pind_server_sqlite.Common
{
    public class AccessToken
    {
        private static string key = "3423f23fj2934i348434t235!@#.comw";
        private static string iv = "7rtw7344t23t34t.";

        public static string getAccessToken(string data)
        {
            return Util.AESEncrypt(data, key, iv);
        }

        public static string getOriginalData(string accesstoken)
        {
            return Util.AESDecrypt(accesstoken, key, iv);
        }
    }
}