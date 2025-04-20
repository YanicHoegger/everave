using System.Security.Cryptography;
using System.Text;

namespace everave.server.Import;

public static class StringEncryption
{
    public static string Encrypt(string plainText, string passphrase)
    {
        using var aes = Aes.Create();
        var key = CreateKey(passphrase, aes.KeySize / 8);
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            using var writer = new StreamWriter(cs);
            writer.Write(plainText);
        }

        var iv = aes.IV;
        var encryptedContent = ms.ToArray();

        var result = new byte[iv.Length + encryptedContent.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

        return Convert.ToBase64String(result);
    }

    public static string Decrypt(string encryptedText, string passphrase)
    {
        var fullCipher = Convert.FromBase64String(encryptedText);

        using var aes = Aes.Create();
        var key = CreateKey(passphrase, aes.KeySize / 8);
        aes.Key = key;

        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(cipher);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);

        return reader.ReadToEnd();
    }

    private static byte[] CreateKey(string passphrase, int keySize)
    {
        using var sha256 = SHA256.Create();
        var key = sha256.ComputeHash(Encoding.UTF8.GetBytes(passphrase));
        Array.Resize(ref key, keySize);
        return key;
    }
}