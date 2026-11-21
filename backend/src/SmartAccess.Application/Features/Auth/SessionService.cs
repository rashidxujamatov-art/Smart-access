using Microsoft.EntityFrameworkCore;
using SmartAccess.Application.Abstractions;
using SmartAccess.Application.Audit;
using SmartAccess.Application.Common;
using SmartAccess.Domain.Audit;
using SmartAccess.Domain.Users;

namespace SmartAccess.Application.Features.Auth;

/// <summary>BR-AUTH-07: faol sessiyalar va ularni tugatish.</summary>
public sealed class SessionService(IAppDbContext db, IAuditLogger audit, IRequestContext request, TimeProvider time)
{
    public async Task<IReadOnlyList<SessionInfo>> ListActiveAsync(CancellationToken ct)
    {
        var now = time.GetUtcNow();
        var currentId = request.User?.SessionId;
        return await db.Sessions
            .Where(s => s.RevokedAt == null && s.ExpiresAt > now && s.User!.DeletedAt == null)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SessionInfo(s.Id, s.UserId, s.User!.Login, s.User.FullName, s.Device, s.Ip,
                s.CreatedAt, s.LastUsedAt, s.Id == currentId))
            .ToListAsync(ct);
    }

    public async Task TerminateAsync(Guid sessionId, CancellationToken ct)
    {
        var session = await db.Sessions.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw AppException.NotFound();

        if (session.RevokedAt is null)
        {
            session.RevokedAt = time.GetUtcNow();
            await db.SaveChangesAsync(ct);
        }

        await audit.WriteAsync(
            new AuditEntry(AuditCategory.Auth, "session_terminated", AuditResult.Success, "session", session.Id.ToString(),
                NewValue: new { user = session.User!.Login, device = session.Device, ip = session.Ip }),
            ct: ct);
    }

    /// <summary>
    /// Access token har so'rovda tekshiriladi: tugatilgan sessiya tokeni DARHOL yaroqsiz (BR-AUTH-07),
    /// o'chirilgan yoki bloklangan foydalanuvchi ham kira olmaydi.
    /// </summary>
    public Task<bool> IsSessionValidAsync(Guid sessionId, Guid userId, CancellationToken ct)
    {
        var now = time.GetUtcNow();
        return db.Sessions.IgnoreQueryFilters().AnyAsync(s =>
            s.Id == sessionId && s.UserId == userId && s.RevokedAt == null && s.ExpiresAt > now
            && s.User!.DeletedAt == null && s.User.Status == UserStatus.Active, ct);
    }
}
