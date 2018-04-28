/*
 * Copyright 2018 Creepsky
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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