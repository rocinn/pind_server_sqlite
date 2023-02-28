using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace pind_server_sqlite.Common
{
    public class FileHelper
    {
        private static string m_strRelativePath = "pindfiles";
        public static string toDisk(string md5, string base64)
        {
            string folder = HttpRuntime.AppDomainAppPath + m_strRelativePath;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string filepath = folder + "/" + md5;
            if (!File.Exists(filepath))
            {
                using (FileStream fs = File.Create(filepath))
                {
                    byte[] arr = Encoding.ASCII.GetBytes(base64);
                    fs.Write(arr, 0, arr.Length);
                }
            }

            return m_strRelativePath + "/" + md5;
        }

        public static string checkFileExist(string md5)
        {
            string folder = HttpRuntime.AppDomainAppPath + m_strRelativePath;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string filepath = folder + "/" + md5;
            if (!File.Exists(filepath))
            {
                return "";
            }

            return m_strRelativePath + "/" + md5;
        }

        public static string GetFileBase64(string relativePath)
        {
            string content = string.Empty;

            string path = HttpRuntime.AppDomainAppPath + relativePath;
            if (File.Exists(path))
            {
                content = File.ReadAllText(path, Encoding.ASCII);
            }

            return content;
        }
    }
}