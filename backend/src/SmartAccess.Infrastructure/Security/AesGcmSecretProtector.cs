using System.Security.Cryptography;
using SmartAccess.Application.Abstractions;

namespace SmartAccess.Infrastructure.Security;

/// <summary>
/// BR-PRIV-01, BR-CAM-01: AES-256-GCM. Natija formati: [versiya 1 bayt][nonce 12][tag 16][shifrlangan matn].
/// Kalit — ENCRYPTION_KEY_BASE64 (32 bayt), faqat muhit o'zgaruvchisida (BR-PRIV-06).
/// </summary>
public sealed class AesGcmSecretProtector : ISecretProtector
{
    private const byte Version = 1;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _key;

    public AesGcmSecretProtector(byte[] key)
    {
        if (key.Length != 32)
        {
            throw new InvalidOperationException("ENCRYPTION_KEY_BASE64 aynan 32 bayt (base64) bo'lishi kerak.");
        }

        _key = key;
    }

    public byte[] Protect(ReadOnlySpan<byte> plaintext)
    {
        var output = new byte[1 + NonceSize + TagSize + plaintext.Length];
        output[0] = Version;
        var nonce = output.AsSpan(1, NonceSize);
        RandomNumberGenerator.Fill(nonce);
        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plaintext, output.AsSpan(1 + NonceSize + TagSize), output.AsSpan(1 + NonceSize, TagSize));
        return output;
    }

    public byte[] Unprotect(ReadOnlySpan<byte> ciphertext)
    {
        if (ciphertext.Length < 1 + NonceSize + TagSize || ciphertext[0] != Version)
        {
            throw new CryptographicException("Shifrlangan ma'lumot formati noto'g'ri.");
        }

        var plain = new byte[ciphertext.Length - 1 - NonceSize - TagSize];
        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(ciphertext.Slice(1, NonceSize), ciphertext[(1 + NonceSize + TagSize)..],
            ciphertext.Slice(1 + NonceSize, TagSize), plain);
        return plain;
    }
}
