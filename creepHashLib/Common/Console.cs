using System;
using System.Data;
using System.Text;

namespace MultiCryptoToolLib.Common
{
    public static class Console
    {
        public static string ReadPassword()
        {
            var password = new StringBuilder();

            while (true)
            {
                var key = System.Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                    break;

                if (key.Key == ConsoleKey.Escape)
                    throw new OperationCanceledException();

                password.Append(key.KeyChar);
            }

            return password.ToString();
        }
    }
}