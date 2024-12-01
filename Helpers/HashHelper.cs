using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AOCNotify.Helpers;

public static class HashHelper
{

    public static string GetSha1Hash(string input)
    {
        return GetSha1Hash(Encoding.UTF8.GetBytes(input));
    }

    public static string GetSha1Hash(byte[] input)
    {
        using var ms = new MemoryStream(input);
        return GetSha1Hash(ms);
    }

    public static string GetSha1Hash(Stream input)
    {
        using var hash = SHA1.Create();
        return BitConverter.ToString(hash.ComputeHash(input)).Replace("-", string.Empty);
    }
}