using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TSE_Core
{
    public static class TypeHelper
    {
        public static string GetMD5(this String data)
        {
            string str = "";
            MD5 md5 = new MD5CryptoServiceProvider();
            Byte[] md5v = md5.ComputeHash(Encoding.Default.GetBytes(data));
            for (int i = 0; i < md5v.Length; i++)
            {
                 str += md5v[i].ToString("x2");
            }
            return str;
        }
    }
}
