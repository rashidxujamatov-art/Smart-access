namespace SmartAccess.Application.Common;

/// <summary>
/// Biznes xatosi. API javobi: {"code": "...", "fields": {...}}.
/// Foydalanuvchiga ko'rsatiladigan matn frontend i18n faylida, kod bo'yicha (CLAUDE.md 0-bo'lim, 9-qoida).
/// </summary>
public sealed class AppException(int status, string code, IReadOnlyDictionary<string, string>? fields = null, object? details = null)
    : Exception(code)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
    public IReadOnlyDictionary<string, string>? Fields { get; } = fields;

    /// <summary>Qo'shimcha ma'lumot (masalan, blok tugash vaqti). Maxfiy ma'lumot qo'yilmaydi.</summary>
    public object? Details { get; } = details;

    public static AppException InvalidCredentials() => new(401, ErrorCodes.InvalidCredentials);
    public static AppException Unauthorized() => new(401, ErrorCodes.Unauthorized);
    public static AppException Forbidden() => new(403, ErrorCodes.Forbidden);
    public static AppException NotFound() => new(404, ErrorCodes.NotFound);

    /// <summary>Bog'liq ma'lumot bor — amal bloklanadi va sababi ko'rsatiladi (CLAUDE.md 9-bo'lim).</summary>
    public static AppException Conflict(string code, object? details = null) => new(409, code, details: details);

    public static AppException Validation(string field, string code) =>
        new(400, ErrorCodes.ValidationError, new Dictionary<string, string> { [field] = code });
}

public static class ErrorCodes
{
    public const string InvalidCredentials = "invalid_credentials";
    public const string AccountLocked = "account_locked";
    public const string Invalid2faCode = "invalid_2fa_code";
    public const string ChallengeExpired = "challenge_expired";
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";
    public const string PasswordChangeRequired = "password_change_required";
    public const string TwoFactorSetupRequired = "two_factor_setup_required";
    public const string ValidationError = "validation_error";
    public const string NotFound = "not_found";

    // Maydon darajasidagi validatsiya kodlari
    public const string Required = "required";
    public const string PasswordTooShort = "password_too_short";
    public const string PasswordSameAsOld = "password_same_as_old";
    public const string WrongPassword = "wrong_password";
    public const string NoPending2fa = "no_pending_2fa";
    public const string TooLong = "too_long";
    public const string NameTaken = "name_taken";
    public const string DepartmentNotFound = "department_not_found";

    // 409 — bog'liq ma'lumot tufayli bloklangan amallar
    public const string DepartmentHasGroups = "department_has_groups";
    public const string GroupHasEmployees = "group_has_employees";
}
