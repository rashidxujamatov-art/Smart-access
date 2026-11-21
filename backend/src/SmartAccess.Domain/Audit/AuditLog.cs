namespace SmartAccess.Domain.Audit;

/// <summary>
/// BR-AUD-02, BR-AUD-03: append-only jurnal. Bazada UPDATE/DELETE trigger bilan taqiqlangan.
/// FullName nusxa sifatida saqlanadi — foydalanuvchi o'chirilsa ham jurnalda qoladi (BR-USR-04).
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public DateTimeOffset Ts { get; set; }
    public Guid? UserId { get; set; }
    public string? UserLogin { get; set; }
    public string? UserFullName { get; set; }
    public string? Role { get; set; }
    public string? Ip { get; set; }
    public string? Device { get; set; }
    public required string Category { get; set; }
    public required string Action { get; set; }
    public string? ObjectType { get; set; }
    public string? ObjectId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public required string Result { get; set; }
}

/// <summary>BR-AUD-04 toifalari.</summary>
public static class AuditCategory
{
    public const string Auth = "auth";
    public const string Gate = "gate";
    public const string Video = "video";
    public const string Settings = "settings";
    public const string Reports = "reports";
    public const string Data = "data";
    public const string Access = "access";
}

public static class AuditResult
{
    public const string Success = "success";
    public const string Failure = "failure";
    public const string Denied = "denied";
}
