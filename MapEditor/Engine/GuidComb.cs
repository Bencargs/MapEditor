using System;
using System.Text;

namespace MapEditor.Engine
{
    public class GuidComb
    {
        public static Guid ToGuid(string src)
        {
            var stringbytes = Encoding.UTF8.GetBytes(src);
            var hashedBytes = new System.Security.Cryptography
                .SHA1CryptoServiceProvider()
                .ComputeHash(stringbytes);
            Array.Resize(ref hashedBytes, 16);
            return new Guid(hashedBytes);
        }
    }
}
