using SmartAccess.Domain.Common;

namespace SmartAccess.Domain.Users;

/// <summary>CLAUDE.md 3-bo'lim: aynan 3 ta rol.</summary>
public enum UserRole
{
    Guard,
    Admin,
    Manager,
}

public enum UserStatus
{
    Active,
    Blocked,
}

public sealed class User : TenantEntity, ISoftDeletable
{
    public required string Login { get; set; }
    public required string FullName { get; set; }
    public UserRole Role { get; set; }
    public required string PasswordHash { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;

    /// <summary>TOTP siri, AES-256-GCM bilan shifrlangan. null — 2FA yoqilmagan.</summary>
    public byte[]? TotpSecretEnc { get; set; }

    /// <summary>2FA sozlanmoqda (sir yaratilgan, kod hali tasdiqlanmagan).</summary>
    public byte[]? PendingTotpSecretEnc { get; set; }

    public bool MustChangePassword { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public int FailedAttempts { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public bool TwoFactorEnabled => TotpSecretEnc is not null;
}
