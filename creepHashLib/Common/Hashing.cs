using System;
using System.Security.Cryptography;
using System.Text;

namespace MultiCryptoToolLib.Common
{
    public static class Hashing
    {
        public static string ToSha256(this string text)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypted = crypt.ComputeHash(Encoding.UTF8.GetBytes(text));
            foreach (var i in crypted)
                hash.Append(i.ToString("x2"));
            return hash.ToString();
        }
    }
}