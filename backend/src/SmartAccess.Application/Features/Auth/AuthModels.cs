using SmartAccess.Domain.Users;

namespace SmartAccess.Application.Features.Auth;

/// <summary>BR-AUTH-04, BR-AUTH-05, BR-USR-02 qiymatlari.</summary>
public sealed class AuthOptions
{
    public const string Section = "Auth";

    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshDays { get; set; } = 7;
    public int RefreshHoursWithoutRemember { get; set; } = 12;
    public int LockoutMinutes { get; set; } = 15;
    public int ChallengeMinutes { get; set; } = 5;
    public int MinPasswordLength { get; set; } = 10;
}

public sealed record LoginRequest(string TenantCode, string Login, string Password, bool RememberMe);

public sealed record TwoFactorRequest(string ChallengeToken, string Code);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record Enable2faRequest(string Code);

/// <summary>Kirish bosqichi: tayyor (ok) yoki 2FA kodi kerak.</summary>
public static class LoginStage
{
    public const string Ok = "ok";
    public const string TwoFactor = "2fa";
}

/// <summary>BR-AUTH-08: rolga qarab boshlang'ich ekran.</summary>
public static class HomeScreens
{
    public static string For(UserRole role) => role switch
    {
        UserRole.Guard => "guard_dashboard",
        UserRole.Admin => "admin_dashboard",
        UserRole.Manager => "manager_dashboard",
        _ => throw new ArgumentOutOfRangeException(nameof(role)),
    };
}

public sealed record UserInfo(
    Guid Id,
    string Login,
    string FullName,
    string Role,
    string Home,
    bool TwoFactorEnabled,
    bool MustChangePassword,
    bool MustSetup2fa);

/// <summary>RefreshToken — faqat ichki (cookie'ga yoziladi yoki native klientga beriladi).</summary>
public sealed record AuthResult(
    string Stage,
    string? AccessToken,
    DateTimeOffset? AccessTokenExpiresAt,
    UserInfo? User,
    string? ChallengeToken,
    string? RefreshToken,
    DateTimeOffset? RefreshTokenExpiresAt);

public sealed record TwoFactorSetup(string Secret, string OtpauthUri);

public sealed record SessionInfo(
    Guid Id,
    Guid UserId,
    string Login,
    string FullName,
    string Device,
    string Ip,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastUsedAt,
    bool IsCurrent);
