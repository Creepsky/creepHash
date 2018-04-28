using System;

namespace MultiCryptoToolLib.Common
{
    public static class Filename
    {
        public static string GetFileExtensionOs()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    return ".exe";
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException("");
            }
        }
    }
}
