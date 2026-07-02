using System.Security.Cryptography;
using System.Text;

namespace DataflowPreps.Core.Pipeline;

internal static class Hasher
{
    public static string Hash(string platform, string rawContent)
    {
        var normalized = $"{platform}:{rawContent.Trim().ToLowerInvariant()}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes)[..16];
    }
}