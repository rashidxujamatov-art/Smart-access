using SmartAccess.Domain.Common;

namespace SmartAccess.Domain.Users;

/// <summary>BR-AUTH-05, BR-AUTH-07: refresh token sessiyasi. Token o'zi saqlanmaydi — faqat SHA-256 xeshi.</summary>
public sealed class Session : TenantEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public required string RefreshTokenHash { get; set; }
    public required string Device { get; set; }
    public required string Ip { get; set; }
    public bool RememberMe { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;
}
