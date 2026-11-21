using System.Text.Json;
using SmartAccess.Application.Abstractions;
using SmartAccess.Domain.Audit;

namespace SmartAccess.Application.Audit;

/// <summary>Audit yozuvi. Maxfiy maydonlar (parol, sir, biometrik) bu yerga HECH QACHON uzatilmaydi.</summary>
public sealed record AuditEntry(
    string Category,
    string Action,
    string Result,
    string? ObjectType = null,
    string? ObjectId = null,
    object? OldValue = null,
    object? NewValue = null);

/// <summary>Kim amal bajargani — so'rovdagi foydalanuvchidan farqli bo'lsa (masalan, login urinishida).</summary>
public sealed record AuditActor(Guid TenantId, Guid? UserId, string? Login, string? FullName, string? Role);

public interface IAuditLogger
{
    /// <summary>Yozuvni qo'shadi va darhol saqlaydi (asosiy amal muvaffaqiyatsiz bo'lsa ham audit qoladi).</summary>
    Task WriteAsync(AuditEntry entry, AuditActor? actor = null, CancellationToken ct = default);
}

public sealed class AuditLogger(IAppDbContext db, IRequestContext request, TimeProvider time) : IAuditLogger
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task WriteAsync(AuditEntry entry, AuditActor? actor = null, CancellationToken ct = default)
    {
        var current = request.User;
        db.AuditLogs.Add(new AuditLog
        {
            Ts = time.GetUtcNow(),
            TenantId = actor?.TenantId ?? current?.TenantId ?? Guid.Empty,
            UserId = actor is not null ? actor.UserId : current?.UserId,
            UserLogin = actor?.Login ?? current?.Login,
            UserFullName = actor?.FullName,
            Role = actor?.Role ?? current?.Role.ToString().ToLowerInvariant(),
            Ip = request.Ip,
            Device = request.Device,
            Category = entry.Category,
            Action = entry.Action,
            ObjectType = entry.ObjectType,
            ObjectId = entry.ObjectId,
            OldValue = entry.OldValue is null ? null : JsonSerializer.Serialize(entry.OldValue, Json),
            NewValue = entry.NewValue is null ? null : JsonSerializer.Serialize(entry.NewValue, Json),
            Result = entry.Result,
        });
        await db.SaveChangesAsync(ct);
    }
}
