using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using SmartAccess.Application.Abstractions;

namespace SmartAccess.Infrastructure.Security;

/// <summary>
/// BR-AUTH-02: Argon2id (OWASP tavsiyasi: m=19 MiB, t=2, p=1).
/// Format (PHC): $argon2id$v=19$m=19456,t=2,p=1$&lt;salt b64&gt;$&lt;hash b64&gt;
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int MemoryKib = 19456;
    private const int Iterations = 2;
    private const int Parallelism = 1;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Compute(password, salt, MemoryKib, Iterations, Parallelism, HashBytes);
        return $"$argon2id$v=19$m={MemoryKib},t={Iterations},p={Parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string encoded)
    {
        var parts = encoded.Split('$');
        if (parts.Length != 6 || parts[1] != "argon2id")
        {
            return false;
        }

        try
        {
            var prm = parts[3].Split(',').Select(p => p.Split('=')).ToDictionary(p => p[0], p => int.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture));
            var salt = Convert.FromBase64String(parts[4]);
            var expected = Convert.FromBase64String(parts[5]);
            var actual = Compute(password, salt, prm["m"], prm["t"], prm["p"], expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    private static byte[] Compute(string password, byte[] salt, int memoryKib, int iterations, int parallelism, int length)
    {
        using var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = memoryKib,
            Iterations = iterations,
            DegreeOfParallelism = parallelism,
        };
        return argon.GetBytes(length);
    }
}
