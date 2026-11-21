using Microsoft.EntityFrameworkCore;
using SmartAccess.Domain.Audit;
using SmartAccess.Domain.Org;
using SmartAccess.Domain.Settings;
using SmartAccess.Domain.Tenants;
using SmartAccess.Domain.Users;

namespace SmartAccess.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Session> Sessions { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Setting> Settings { get; }
    DbSet<Department> Departments { get; }
    DbSet<Group> Groups { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>BR-AUTH-02: Argon2id.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

/// <summary>BR-PRIV-01, BR-CAM-01: AES-256-GCM. Kalit faqat muhit o'zgaruvchisida (BR-PRIV-06).</summary>
public interface ISecretProtector
{
    byte[] Protect(ReadOnlySpan<byte> plaintext);
    byte[] Unprotect(ReadOnlySpan<byte> ciphertext);
}

/// <summary>BR-AUTH-06: TOTP (RFC 6238), 6 xonali kod.</summary>
public interface ITotpService
{
    byte[] GenerateSecret();
    bool Verify(byte[] secret, string code, DateTimeOffset now);
    string ToBase32(byte[] secret);
    string BuildUri(byte[] secret, string account, string issuer);
}

public interface ITokenService
{
    AccessToken CreateAccessToken(User user, Session session, AccessRestriction restriction);
    string CreateChallengeToken(Guid userId, Guid tenantId, bool rememberMe);
    ChallengeClaims? ReadChallengeToken(string token);
    string NewRefreshToken();
    string HashRefreshToken(string refreshToken);
}

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

public sealed record ChallengeClaims(Guid UserId, Guid TenantId, bool RememberMe);

/// <summary>Parolni almashtirish (BR-USR-02) yoki 2FA sozlash (BR-AUTH-06) kerak bo'lsa, token cheklangan bo'ladi.</summary>
[Flags]
public enum AccessRestriction
{
    None = 0,
    MustChangePassword = 1,
    MustSetup2fa = 2,
}

public sealed record CurrentUser(Guid UserId, Guid TenantId, Guid SessionId, UserRole Role, string Login);

/// <summary>Joriy so'rov: IP, qurilma va (bo'lsa) kirgan foydalanuvchi.</summary>
public interface IRequestContext
{
    string Ip { get; }
    string Device { get; }
    CurrentUser? User { get; }
}

/// <summary>AppDbContext tenant filtri uchun.</summary>
public interface ITenantProvider
{
    Guid? TenantId { get; }
}
