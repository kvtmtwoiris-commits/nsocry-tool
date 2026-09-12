using System.Security.Cryptography;
using System.Text;

namespace NSOCryPro.Services;

public static class CredentialProtector
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("NSOCryPro.Credentials.v1");

    public static string Protect(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var bytes = Encoding.UTF8.GetBytes(value);
        var encrypted = ProtectedData.Protect(bytes, Entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    public static string Unprotect(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        try
        {
            var encrypted = Convert.FromBase64String(value);
            var bytes = ProtectedData.Unprotect(encrypted, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }
}
