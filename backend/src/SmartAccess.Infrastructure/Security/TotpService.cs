using OtpNet;
using SmartAccess.Application.Abstractions;

namespace SmartAccess.Infrastructure.Security;

/// <summary>BR-AUTH-06: TOTP, 30 soniya, 6 raqam; soat farqi uchun ±1 qadam.</summary>
public sealed class TotpService : ITotpService
{
    public byte[] GenerateSecret() => KeyGeneration.GenerateRandomKey(20);

    public bool Verify(byte[] secret, string code, DateTimeOffset now)
    {
        code = code.Trim().Replace(" ", "", StringComparison.Ordinal);
        if (code.Length != 6 || !code.All(char.IsAsciiDigit))
        {
            return false;
        }

        return new Totp(secret).VerifyTotp(now.UtcDateTime, code, out _, new VerificationWindow(previous: 1, future: 1));
    }

    public string ToBase32(byte[] secret) => Base32Encoding.ToString(secret);

    public string BuildUri(byte[] secret, string account, string issuer) =>
        $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(account)}" +
        $"?secret={ToBase32(secret)}&issuer={Uri.EscapeDataString(issuer)}&digits=6&period=30";
}
