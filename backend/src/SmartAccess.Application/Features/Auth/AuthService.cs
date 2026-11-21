using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartAccess.Application.Abstractions;
using SmartAccess.Application.Audit;
using SmartAccess.Application.Common;
using SmartAccess.Application.Settings;
using SmartAccess.Domain.Audit;
using SmartAccess.Domain.Settings;
using SmartAccess.Domain.Users;

namespace SmartAccess.Application.Features.Auth;

/// <summary>CLAUDE.md 4.1 — BR-AUTH-01..09, BR-USR-02.</summary>
public sealed class AuthService(
    IAppDbContext db,
    IPasswordHasher hasher,
    ITokenService tokens,
    ITotpService totp,
    ISecretProtector protector,
    IAuditLogger audit,
    IRequestContext request,
    SettingsReader settings,
    TimeProvider time,
    IOptions<AuthOptions> options)
{
    private const string TotpIssuer = "Smart Access";
    private readonly AuthOptions _opt = options.Value;

    // ---------------- Login (BR-AUTH-03, BR-AUTH-04) ----------------

    public async Task<AuthResult> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var tenantCode = (req.TenantCode ?? "").Trim().ToUpperInvariant();
        var login = NormalizeLogin(req.Login);

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Code == tenantCode, ct);
        if (tenant is null)
        {
            await AuditLoginFailure(Guid.Empty, null, login, "unknown_tenant", ct);
            throw AppException.InvalidCredentials();
        }

        var user = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.Login == login && u.DeletedAt == null, ct);
        if (user is null)
        {
            await AuditLoginFailure(tenant.Id, null, login, "unknown_login", ct);
            throw AppException.InvalidCredentials();
        }

        await EnsureNotLockedAsync(user, ct);

        if (string.IsNullOrEmpty(req.Password) || !hasher.Verify(req.Password, user.PasswordHash))
        {
            await RegisterFailureAsync(user, "wrong_password", ct);
            throw AppException.InvalidCredentials();
        }

        if (user.Status != UserStatus.Active)
        {
            await AuditLoginFailure(tenant.Id, user, login, "user_blocked", ct);
            throw AppException.InvalidCredentials();
        }

        user.FailedAttempts = 0;
        user.LockedUntil = null;

        if (user.TwoFactorEnabled)
        {
            await db.SaveChangesAsync(ct);
            await audit.WriteAsync(
                new AuditEntry(AuditCategory.Auth, "login_password_ok_2fa_required", AuditResult.Success, "user", user.Id.ToString()),
                Actor(user), ct);
            return new AuthResult(LoginStage.TwoFactor, null, null, null,
                tokens.CreateChallengeToken(user.Id, user.TenantId, req.RememberMe), null, null);
        }

        return await CompleteLoginAsync(user, req.RememberMe, ct);
    }

    // ---------------- 2FA (BR-AUTH-06) ----------------

    public async Task<AuthResult> VerifyTwoFactorAsync(TwoFactorRequest req, CancellationToken ct)
    {
        var claims = tokens.ReadChallengeToken(req.ChallengeToken ?? "")
            ?? throw new AppException(401, ErrorCodes.ChallengeExpired);

        var user = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == claims.UserId && u.TenantId == claims.TenantId && u.DeletedAt == null, ct)
            ?? throw new AppException(401, ErrorCodes.ChallengeExpired);

        await EnsureNotLockedAsync(user, ct);

        if (user.Status != UserStatus.Active || user.TotpSecretEnc is null)
        {
            throw new AppException(401, ErrorCodes.ChallengeExpired);
        }

        var secret = protector.Unprotect(user.TotpSecretEnc);
        if (!totp.Verify(secret, req.Code ?? "", time.GetUtcNow()))
        {
            await RegisterFailureAsync(user, "wrong_2fa_code", ct);
            throw new AppException(401, ErrorCodes.Invalid2faCode);
        }

        user.FailedAttempts = 0;
        return await CompleteLoginAsync(user, claims.RememberMe, ct);
    }

    // ---------------- Refresh (BR-AUTH-05) ----------------

    public async Task<AuthResult> RefreshAsync(string? refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw AppException.Unauthorized();
        }

        var hash = tokens.HashRefreshToken(refreshToken);
        var now = time.GetUtcNow();
        var session = await db.Sessions.IgnoreQueryFilters()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == hash, ct);

        if (session is null || !session.IsActive(now))
        {
            throw AppException.Unauthorized();
        }

        var user = session.User!;
        if (user.DeletedAt is not null || user.Status != UserStatus.Active)
        {
            session.RevokedAt = now;
            await db.SaveChangesAsync(ct);
            throw AppException.Unauthorized();
        }

        // Rotatsiya: eski refresh token endi ishlamaydi.
        var newRefresh = tokens.NewRefreshToken();
        session.RefreshTokenHash = tokens.HashRefreshToken(newRefresh);
        session.LastUsedAt = now;
        await db.SaveChangesAsync(ct);

        var access = tokens.CreateAccessToken(user, session, await RestrictionForAsync(user, ct));
        return new AuthResult(LoginStage.Ok, access.Token, access.ExpiresAt, await ToInfoAsync(user, ct),
            null, newRefresh, session.ExpiresAt);
    }

    // ---------------- Logout (BR-AUTH-09) ----------------

    public async Task LogoutAsync(CurrentUser current, CancellationToken ct)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == current.SessionId, ct);
        if (session is not null && session.RevokedAt is null)
        {
            session.RevokedAt = time.GetUtcNow();
            await db.SaveChangesAsync(ct);
        }

        await audit.WriteAsync(
            new AuditEntry(AuditCategory.Auth, "logout", AuditResult.Success, "session", current.SessionId.ToString()),
            ct: ct);
    }

    // ---------------- Me ----------------

    public async Task<UserInfo> MeAsync(CurrentUser current, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == current.UserId, ct)
            ?? throw AppException.Unauthorized();
        return await ToInfoAsync(user, ct);
    }

    // ---------------- Parolni almashtirish (BR-USR-02) ----------------

    public async Task<AuthResult> ChangePasswordAsync(CurrentUser current, ChangePasswordRequest req, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == current.UserId, ct)
            ?? throw AppException.Unauthorized();

        if (string.IsNullOrEmpty(req.CurrentPassword) || !hasher.Verify(req.CurrentPassword, user.PasswordHash))
        {
            await audit.WriteAsync(
                new AuditEntry(AuditCategory.Auth, "password_change", AuditResult.Failure, "user", user.Id.ToString()),
                ct: ct);
            throw AppException.Validation("currentPassword", ErrorCodes.WrongPassword);
        }

        ValidateNewPassword(req.NewPassword, "newPassword");
        if (req.NewPassword == req.CurrentPassword)
        {
            throw AppException.Validation("newPassword", ErrorCodes.PasswordSameAsOld);
        }

        var now = time.GetUtcNow();
        user.PasswordHash = hasher.Hash(req.NewPassword);
        user.MustChangePassword = false;

        // Boshqa barcha sessiyalar yopiladi — parol o'zgargani boshqa qurilmalarga ham tegishli.
        var others = await db.Sessions
            .Where(s => s.UserId == user.Id && s.Id != current.SessionId && s.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var s in others)
        {
            s.RevokedAt = now;
        }

        await db.SaveChangesAsync(ct);
        await audit.WriteAsync(
            new AuditEntry(AuditCategory.Auth, "password_change", AuditResult.Success, "user", user.Id.ToString()),
            ct: ct);

        return await ReissueAccessAsync(user, current.SessionId, ct);
    }

    // ---------------- 2FA ni yoqish (BR-AUTH-06) ----------------

    public async Task<TwoFactorSetup> BeginTwoFactorSetupAsync(CurrentUser current, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == current.UserId, ct)
            ?? throw AppException.Unauthorized();

        var secret = totp.GenerateSecret();
        user.PendingTotpSecretEnc = protector.Protect(secret);
        await db.SaveChangesAsync(ct);

        var tenant = await db.Tenants.FirstAsync(t => t.Id == user.TenantId, ct);
        return new TwoFactorSetup(totp.ToBase32(secret), totp.BuildUri(secret, $"{tenant.Code}:{user.Login}", TotpIssuer));
    }

    public async Task<AuthResult> EnableTwoFactorAsync(CurrentUser current, Enable2faRequest req, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == current.UserId, ct)
            ?? throw AppException.Unauthorized();

        if (user.PendingTotpSecretEnc is null)
        {
            throw AppException.Validation("code", ErrorCodes.NoPending2fa);
        }

        var secret = protector.Unprotect(user.PendingTotpSecretEnc);
        if (!totp.Verify(secret, req.Code ?? "", time.GetUtcNow()))
        {
            await audit.WriteAsync(
                new AuditEntry(AuditCategory.Auth, "2fa_enable", AuditResult.Failure, "user", user.Id.ToString()), ct: ct);
            throw new AppException(400, ErrorCodes.Invalid2faCode);
        }

        user.TotpSecretEnc = user.PendingTotpSecretEnc;
        user.PendingTotpSecretEnc = null;
        await db.SaveChangesAsync(ct);
        await audit.WriteAsync(
            new AuditEntry(AuditCategory.Auth, "2fa_enable", AuditResult.Success, "user", user.Id.ToString()), ct: ct);

        return await ReissueAccessAsync(user, current.SessionId, ct);
    }

    // ---------------- Yordamchilar ----------------

    public void ValidateNewPassword(string? password, string field)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw AppException.Validation(field, ErrorCodes.Required);
        }

        if (password.Length < _opt.MinPasswordLength)
        {
            throw AppException.Validation(field, ErrorCodes.PasswordTooShort);
        }
    }

    public static string NormalizeLogin(string? login) => (login ?? "").Trim().ToLowerInvariant();

    private async Task<AuthResult> CompleteLoginAsync(User user, bool rememberMe, CancellationToken ct)
    {
        var now = time.GetUtcNow();
        var refresh = tokens.NewRefreshToken();
        var session = new Session
        {
            TenantId = user.TenantId,
            UserId = user.Id,
            RefreshTokenHash = tokens.HashRefreshToken(refresh),
            Device = request.Device,
            Ip = request.Ip,
            RememberMe = rememberMe,
            ExpiresAt = rememberMe
                ? now.AddDays(_opt.RefreshDays)
                : now.AddHours(_opt.RefreshHoursWithoutRemember),
            LastUsedAt = now,
        };
        db.Sessions.Add(session);
        user.LastLoginAt = now;
        await db.SaveChangesAsync(ct);

        await audit.WriteAsync(
            new AuditEntry(AuditCategory.Auth, "login", AuditResult.Success, "session", session.Id.ToString()),
            Actor(user), ct);

        var access = tokens.CreateAccessToken(user, session, await RestrictionForAsync(user, ct));
        return new AuthResult(LoginStage.Ok, access.Token, access.ExpiresAt, await ToInfoAsync(user, ct),
            null, refresh, session.ExpiresAt);
    }

    private async Task<AuthResult> ReissueAccessAsync(User user, Guid sessionId, CancellationToken ct)
    {
        var session = await db.Sessions.FirstAsync(s => s.Id == sessionId, ct);
        var access = tokens.CreateAccessToken(user, session, await RestrictionForAsync(user, ct));
        return new AuthResult(LoginStage.Ok, access.Token, access.ExpiresAt, await ToInfoAsync(user, ct),
            null, null, null);
    }

    private async Task<AccessRestriction> RestrictionForAsync(User user, CancellationToken ct)
    {
        var r = AccessRestriction.None;
        if (user.MustChangePassword)
        {
            r |= AccessRestriction.MustChangePassword;
        }

        if (user.Role == UserRole.Admin && !user.TwoFactorEnabled
            && await settings.GetBoolAsync(user.TenantId, SettingKeys.SecurityRequire2faForAdmin, ct))
        {
            r |= AccessRestriction.MustSetup2fa;
        }

        return r;
    }

    private async Task<UserInfo> ToInfoAsync(User user, CancellationToken ct)
    {
        var r = await RestrictionForAsync(user, ct);
        return new UserInfo(user.Id, user.Login, user.FullName, RoleCode(user.Role), HomeScreens.For(user.Role),
            user.TwoFactorEnabled,
            r.HasFlag(AccessRestriction.MustChangePassword),
            r.HasFlag(AccessRestriction.MustSetup2fa));
    }

    private async Task EnsureNotLockedAsync(User user, CancellationToken ct)
    {
        var now = time.GetUtcNow();
        if (user.LockedUntil is { } until && until > now)
        {
            await AuditLoginFailure(user.TenantId, user, user.Login, "account_locked", ct);
            throw new AppException(423, ErrorCodes.AccountLocked, details: new { lockedUntil = until });
        }
    }

    /// <summary>BR-AUTH-04: ketma-ket N xato → 15 daqiqa blok.</summary>
    private async Task RegisterFailureAsync(User user, string reason, CancellationToken ct)
    {
        var limit = await settings.GetIntAsync(user.TenantId, SettingKeys.SecurityLockoutAttempts, ct);
        user.FailedAttempts++;
        var locked = false;
        if (user.FailedAttempts >= limit)
        {
            user.LockedUntil = time.GetUtcNow().AddMinutes(_opt.LockoutMinutes);
            user.FailedAttempts = 0;
            locked = true;
        }

        await db.SaveChangesAsync(ct);
        await AuditLoginFailure(user.TenantId, user, user.Login, reason, ct);
        if (locked)
        {
            await audit.WriteAsync(
                new AuditEntry(AuditCategory.Auth, "account_locked", AuditResult.Denied, "user", user.Id.ToString(),
                    NewValue: new { lockedUntil = user.LockedUntil }),
                Actor(user), ct);
        }
    }

    private Task AuditLoginFailure(Guid tenantId, User? user, string login, string reason, CancellationToken ct) =>
        audit.WriteAsync(
            new AuditEntry(AuditCategory.Auth, "login", AuditResult.Failure, "user", user?.Id.ToString(),
                NewValue: new { reason }),
            new AuditActor(tenantId, user?.Id, login, user?.FullName, user is null ? null : RoleCode(user.Role)),
            ct);

    private static AuditActor Actor(User user) =>
        new(user.TenantId, user.Id, user.Login, user.FullName, RoleCode(user.Role));

    public static string RoleCode(UserRole role) => role.ToString().ToLowerInvariant();
}
