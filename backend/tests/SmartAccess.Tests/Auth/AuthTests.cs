using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartAccess.Domain.Audit;
using SmartAccess.Domain.Users;
using SmartAccess.Tests.Infrastructure;

namespace SmartAccess.Tests.Auth;

/// <summary>CLAUDE.md 4.1 (BR-AUTH-01..09), BR-USR-02, BR-AUD-03, BR-RBAC-04.</summary>
[Collection(DbCollection.Name)]
public sealed class AuthTests(PostgresFixture pg) : IDisposable
{
    private readonly ApiFactory _api = new(pg);

    public void Dispose() => _api.Dispose();

    private Task<List<AuditLog>> AuditFor(Guid tenantId) =>
        _api.WithDb((db, _) => db.AuditLogs.IgnoreQueryFilters().Where(a => a.TenantId == tenantId).OrderBy(a => a.Ts).ToListAsync());

    // ---------------- BR-AUTH-01 ----------------

    [Theory]
    [InlineData("/api/v1/auth/register")]
    [InlineData("/api/v1/auth/signup")]
    public async Task test_BR_AUTH_01_no_self_registration_endpoint(string path)
    {
        var r = await _api.CreateClient().PostAsJsonAsync(path, new { login = "x", password = "yyyyyyyyyyyy" });
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    // ---------------- BR-AUTH-02 ----------------

    [Fact]
    public async Task test_BR_AUTH_02_password_hashed_with_argon2id_and_never_logged()
    {
        var t = await _api.CreateTenantAsync();
        var u = await _api.CreateUserAsync(t, "hash1", UserRole.Guard);
        const string Attempt = "Wrong-But-Secret-Value-42";
        await _api.CreateClient().Login(t.Code, "hash1", Attempt);

        var stored = await _api.WithDb((db, _) => db.Users.IgnoreQueryFilters().Where(x => x.Id == u.Id).Select(x => x.PasswordHash).SingleAsync());
        Assert.StartsWith("$argon2id$v=19$", stored, StringComparison.Ordinal);
        Assert.DoesNotContain(ApiFactory.TestPassword, stored, StringComparison.Ordinal);

        var audit = await AuditFor(t.Id);
        Assert.NotEmpty(audit);
        Assert.All(audit, a =>
        {
            Assert.DoesNotContain(Attempt, a.NewValue ?? "", StringComparison.Ordinal);
            Assert.DoesNotContain(ApiFactory.TestPassword, a.NewValue ?? "", StringComparison.Ordinal);
        });
    }

    // ---------------- BR-AUTH-03 ----------------

    [Fact]
    public async Task test_BR_AUTH_03_same_generic_error_for_wrong_tenant_login_or_password()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "gen1", UserRole.Guard);
        var c = _api.CreateClient();

        var wrongPassword = await c.Login(t.Code, "gen1", "nope-nope-nope");
        var wrongLogin = await c.Login(t.Code, "nobody", ApiFactory.TestPassword);
        var wrongTenant = await c.Login("NO-SUCH-TENANT", "gen1", ApiFactory.TestPassword);

        foreach (var r in new[] { wrongPassword, wrongLogin, wrongTenant })
        {
            Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
            Assert.Equal("""{"code":"invalid_credentials","fields":null,"details":null}""", await r.Content.ReadAsStringAsync());
        }
    }

    [Fact]
    public async Task test_BR_AUTH_03_tenant_code_and_login_are_case_insensitive()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "case1", UserRole.Guard);

        var body = await _api.CreateClient().LoginOk(t.Code.ToLowerInvariant(), "  CASE1 ");
        Assert.Equal("ok", body.Stage);
    }

    [Fact]
    public async Task test_BR_AUTH_03_blocked_user_gets_generic_error()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "blk1", UserRole.Guard, status: UserStatus.Blocked);

        var r = await _api.CreateClient().Login(t.Code, "blk1", ApiFactory.TestPassword);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
        Assert.Equal("invalid_credentials", await r.ErrorCode());
    }

    // ---------------- BR-AUTH-04 ----------------

    [Fact]
    public async Task test_BR_AUTH_04_five_failures_lock_account_for_15_minutes_and_each_is_audited()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "lock1", UserRole.Guard);
        var c = _api.Native();

        for (var i = 0; i < 5; i++)
        {
            var r = await c.Login(t.Code, "lock1", "wrong-password-" + i);
            Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
        }

        // To'g'ri parol bilan ham blok.
        var locked = await c.Login(t.Code, "lock1", ApiFactory.TestPassword);
        Assert.Equal((HttpStatusCode)423, locked.StatusCode);
        Assert.Equal("account_locked", await locked.ErrorCode());

        var audit = await AuditFor(t.Id);
        var failures = audit.Where(a => a.Action == "login" && a.Result == AuditResult.Failure).ToList();
        Assert.True(failures.Count >= 6);
        Assert.All(failures, a =>
        {
            Assert.Equal("lock1", a.UserLogin);
            Assert.False(string.IsNullOrEmpty(a.Ip));
            Assert.Equal("SmartAccessTests/1.0", a.Device);
        });
        Assert.Contains(audit, a => a.Action == "account_locked");

        _api.Time.Advance(TimeSpan.FromMinutes(14));
        Assert.Equal((HttpStatusCode)423, (await c.Login(t.Code, "lock1", ApiFactory.TestPassword)).StatusCode);

        _api.Time.Advance(TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1));
        Assert.Equal(HttpStatusCode.OK, (await c.Login(t.Code, "lock1", ApiFactory.TestPassword)).StatusCode);
    }

    [Fact]
    public async Task test_BR_AUTH_04_successful_login_resets_failure_counter()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "lock2", UserRole.Guard);
        var c = _api.CreateClient();

        for (var i = 0; i < 4; i++)
        {
            await c.Login(t.Code, "lock2", "bad-password");
        }

        await c.LoginOk(t.Code, "lock2");

        for (var i = 0; i < 4; i++)
        {
            await c.Login(t.Code, "lock2", "bad-password");
        }

        Assert.Equal(HttpStatusCode.OK, (await c.Login(t.Code, "lock2", ApiFactory.TestPassword)).StatusCode);
    }

    // ---------------- BR-AUTH-05 ----------------

    [Fact]
    public async Task test_BR_AUTH_05_access_token_15_minutes_refresh_7_days_or_12_hours()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "tok1", UserRole.Guard);
        var now = _api.Time.GetUtcNow();

        var remembered = await _api.Native().LoginOk(t.Code, "tok1", remember: true);
        Assert.Equal(now.AddMinutes(15), remembered.AccessTokenExpiresAt);
        Assert.Equal(now.AddDays(7), remembered.RefreshTokenExpiresAt);
        Assert.False(string.IsNullOrEmpty(remembered.RefreshToken));

        var notRemembered = await _api.Native().LoginOk(t.Code, "tok1", remember: false);
        Assert.Equal(now.AddHours(12), notRemembered.RefreshTokenExpiresAt);
    }

    [Fact]
    public async Task test_BR_AUTH_05_web_client_gets_refresh_token_only_as_httponly_cookie()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "tok2", UserRole.Guard);

        var r = await _api.CreateClient().Login(t.Code, "tok2", ApiFactory.TestPassword);
        var body = await r.Content.ReadFromJsonAsync<LoginBody>();
        Assert.Null(body!.RefreshToken);

        var cookie = Assert.Single(r.Headers.GetValues("Set-Cookie"));
        Assert.StartsWith("sa_refresh=", cookie, StringComparison.Ordinal);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/v1/auth", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task test_BR_AUTH_05_refresh_rotates_token_and_old_one_stops_working()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "tok3", UserRole.Guard);
        var c = _api.Native();
        var login = await c.LoginOk(t.Code, "tok3");

        var r1 = await c.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = login.RefreshToken });
        r1.EnsureSuccessStatusCode();
        var refreshed = await r1.Content.ReadFromJsonAsync<LoginBody>();
        Assert.NotEqual(login.RefreshToken, refreshed!.RefreshToken);
        Assert.False(string.IsNullOrEmpty(refreshed.AccessToken));

        var reuse = await c.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = login.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
    }

    [Fact]
    public async Task test_BR_AUTH_05_refresh_fails_after_expiry()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "tok4", UserRole.Guard);
        var c = _api.Native();
        var login = await c.LoginOk(t.Code, "tok4", remember: false);

        _api.Time.Advance(TimeSpan.FromHours(12) + TimeSpan.FromSeconds(1));
        var r = await c.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = login.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    // ---------------- BR-AUTH-06 ----------------

    [Fact]
    public async Task test_BR_AUTH_06_totp_code_required_after_correct_password()
    {
        var t = await _api.CreateTenantAsync();
        var secret = OtpNet.KeyGeneration.GenerateRandomKey(20);
        await _api.CreateUserAsync(t, "otp1", UserRole.Admin, totpSecret: secret);
        var c = _api.Native();

        var step1 = await c.LoginOk(t.Code, "otp1");
        Assert.Equal("2fa", step1.Stage);
        Assert.Null(step1.AccessToken);
        Assert.Null(step1.RefreshToken);
        Assert.False(string.IsNullOrEmpty(step1.ChallengeToken));

        var bad = await c.PostAsJsonAsync("/api/v1/auth/2fa", new { challengeToken = step1.ChallengeToken, code = "000000" });
        Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);
        Assert.Equal("invalid_2fa_code", await bad.ErrorCode());

        var ok = await c.PostAsJsonAsync("/api/v1/auth/2fa", new { challengeToken = step1.ChallengeToken, code = _api.TotpNow(secret) });
        ok.EnsureSuccessStatusCode();
        var body = await ok.Content.ReadFromJsonAsync<LoginBody>();
        Assert.Equal("ok", body!.Stage);
        Assert.True(body.User!.TwoFactorEnabled);
        Assert.False(string.IsNullOrEmpty(body.AccessToken));
    }

    [Fact]
    public async Task test_BR_AUTH_06_challenge_token_expires_after_5_minutes()
    {
        var t = await _api.CreateTenantAsync();
        var secret = OtpNet.KeyGeneration.GenerateRandomKey(20);
        await _api.CreateUserAsync(t, "otp2", UserRole.Admin, totpSecret: secret);
        var c = _api.Native();
        var step1 = await c.LoginOk(t.Code, "otp2");

        _api.Time.Advance(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(1));
        var r = await c.PostAsJsonAsync("/api/v1/auth/2fa", new { challengeToken = step1.ChallengeToken, code = _api.TotpNow(secret) });
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
        Assert.Equal("challenge_expired", await r.ErrorCode());
    }

    [Fact]
    public async Task test_BR_AUTH_06_admin_must_enable_2fa_when_required_by_settings()
    {
        var t = await _api.CreateTenantAsync(require2faForAdmin: true);
        await _api.CreateUserAsync(t, "adm2fa", UserRole.Admin);
        var c = _api.Native();

        var login = await c.LoginOk(t.Code, "adm2fa");
        Assert.True(login.User!.MustSetup2fa);
        c.WithToken(login.AccessToken);

        var blocked = await c.GetAsync(new Uri("/api/v1/sessions", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Forbidden, blocked.StatusCode);
        Assert.Equal("two_factor_setup_required", await blocked.ErrorCode());

        var setup = await (await c.PostAsync(new Uri("/api/v1/auth/2fa/setup", UriKind.Relative), null))
            .Content.ReadFromJsonAsync<SetupBody>();
        Assert.StartsWith("otpauth://totp/", setup!.OtpauthUri, StringComparison.Ordinal);
        var secret = OtpNet.Base32Encoding.ToBytes(setup.Secret);

        var enable = await c.PostAsJsonAsync("/api/v1/auth/2fa/enable", new { code = _api.TotpNow(secret) });
        enable.EnsureSuccessStatusCode();
        var enabled = await enable.Content.ReadFromJsonAsync<LoginBody>();
        Assert.False(enabled!.User!.MustSetup2fa);
        Assert.True(enabled.User.TwoFactorEnabled);

        c.WithToken(enabled.AccessToken);
        Assert.Equal(HttpStatusCode.OK, (await c.GetAsync(new Uri("/api/v1/sessions", UriKind.Relative))).StatusCode);

        // Keyingi kirishda kod so'raladi.
        Assert.Equal("2fa", (await _api.Native().LoginOk(t.Code, "adm2fa")).Stage);
    }

    [Fact]
    public async Task test_BR_AUTH_06_totp_secret_stored_encrypted()
    {
        var t = await _api.CreateTenantAsync();
        var secret = OtpNet.KeyGeneration.GenerateRandomKey(20);
        var u = await _api.CreateUserAsync(t, "otp3", UserRole.Admin, totpSecret: secret);

        var stored = await _api.WithDb((db, _) => db.Users.IgnoreQueryFilters().Where(x => x.Id == u.Id).Select(x => x.TotpSecretEnc).SingleAsync());
        Assert.NotNull(stored);
        Assert.False(stored.AsSpan().IndexOf(secret) >= 0);
    }

    // ---------------- BR-AUTH-07 ----------------

    [Fact]
    public async Task test_BR_AUTH_07_admin_terminates_session_and_token_is_invalid_immediately()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "adm7", UserRole.Admin);
        await _api.CreateUserAsync(t, "grd7", UserRole.Guard);
        var admin = _api.Native();
        admin.WithToken((await admin.LoginOk(t.Code, "adm7")).AccessToken);
        var guard = _api.Native();
        var guardLogin = await guard.LoginOk(t.Code, "grd7");
        guard.WithToken(guardLogin.AccessToken);

        Assert.Equal(HttpStatusCode.OK, (await guard.GetAsync(new Uri("/api/v1/auth/me", UriKind.Relative))).StatusCode);

        var sessions = await admin.GetFromJsonAsync<List<SessionBody>>("/api/v1/sessions");
        var guardSession = Assert.Single(sessions!, s => s.Login == "grd7");
        Assert.Equal("SmartAccessTests/1.0", guardSession.Device);
        Assert.False(string.IsNullOrEmpty(guardSession.Ip));
        Assert.Contains(sessions!, s => s.Login == "adm7" && s.IsCurrent);

        var del = await admin.DeleteAsync(new Uri($"/api/v1/sessions/{guardSession.Id}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await guard.GetAsync(new Uri("/api/v1/auth/me", UriKind.Relative))).StatusCode);
        var refresh = await guard.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = guardLogin.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);

        Assert.Contains(await AuditFor(t.Id), a => a.Action == "session_terminated" && a.UserLogin == "adm7");
    }

    [Theory]
    [InlineData(UserRole.Guard)]
    [InlineData(UserRole.Manager)]
    public async Task test_BR_AUTH_07_only_admin_manages_sessions_others_get_403_and_audit(UserRole role)
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "nonadm", role);
        var c = _api.Native();
        c.WithToken((await c.LoginOk(t.Code, "nonadm")).AccessToken);

        var list = await c.GetAsync(new Uri("/api/v1/sessions", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        var del = await c.DeleteAsync(new Uri($"/api/v1/sessions/{Guid.NewGuid()}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Forbidden, del.StatusCode);

        var denied = (await AuditFor(t.Id)).Where(a => a.Action == "forbidden" && a.Result == AuditResult.Denied).ToList();
        Assert.Equal(2, denied.Count);
        Assert.All(denied, a => Assert.Equal("nonadm", a.UserLogin));
    }

    [Fact]
    public async Task test_BR_AUTH_07_admin_sees_only_own_tenant_sessions()
    {
        var a = await _api.CreateTenantAsync();
        var b = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(a, "adma", UserRole.Admin);
        await _api.CreateUserAsync(b, "grdb", UserRole.Guard);
        var otherLogin = await _api.Native().LoginOk(b.Code, "grdb");
        var admin = _api.Native();
        admin.WithToken((await admin.LoginOk(a.Code, "adma")).AccessToken);

        var sessions = await admin.GetFromJsonAsync<List<SessionBody>>("/api/v1/sessions");
        Assert.DoesNotContain(sessions!, s => s.Login == "grdb");

        var otherSessionId = await _api.WithDb((db, _) => db.Sessions.IgnoreQueryFilters()
            .Where(s => s.TenantId == b.Id).Select(s => s.Id).SingleAsync());
        var del = await admin.DeleteAsync(new Uri($"/api/v1/sessions/{otherSessionId}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NotFound, del.StatusCode);
        Assert.NotNull(otherLogin.AccessToken);
    }

    [Fact]
    public async Task test_BR_AUTH_07_unauthenticated_request_gets_401()
    {
        var r = await _api.CreateClient().GetAsync(new Uri("/api/v1/auth/me", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
        Assert.Equal("unauthorized", await r.ErrorCode());
    }

    // ---------------- BR-AUTH-08 ----------------

    [Theory]
    [InlineData(UserRole.Guard, "guard", "guard_dashboard")]
    [InlineData(UserRole.Admin, "admin", "admin_dashboard")]
    [InlineData(UserRole.Manager, "manager", "manager_dashboard")]
    public async Task test_BR_AUTH_08_role_determines_home_screen(UserRole role, string code, string home)
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "home1", role);
        var c = _api.Native();

        var login = await c.LoginOk(t.Code, "home1");
        Assert.Equal(code, login.User!.Role);
        Assert.Equal(home, login.User.Home);

        c.WithToken(login.AccessToken);
        var me = await c.GetFromJsonAsync<UserBody>("/api/v1/auth/me");
        Assert.Equal(home, me!.Home);
    }

    // ---------------- BR-AUTH-09 ----------------

    [Fact]
    public async Task test_BR_AUTH_09_logout_revokes_refresh_token_and_is_audited()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "out1", UserRole.Guard);
        var c = _api.Native();
        var login = await c.LoginOk(t.Code, "out1");
        c.WithToken(login.AccessToken);

        var r = await c.PostAsync(new Uri("/api/v1/auth/logout", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.NoContent, r.StatusCode);

        var refresh = await c.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = login.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync(new Uri("/api/v1/auth/me", UriKind.Relative))).StatusCode);
        Assert.Contains(await AuditFor(t.Id), a => a.Action == "logout" && a.UserLogin == "out1");
    }

    // ---------------- BR-USR-02 ----------------

    [Fact]
    public async Task test_BR_USR_02_password_change_required_on_first_login()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "first1", UserRole.Admin, mustChange: true);
        var c = _api.Native();
        var login = await c.LoginOk(t.Code, "first1");
        Assert.True(login.User!.MustChangePassword);
        c.WithToken(login.AccessToken);

        var blocked = await c.GetAsync(new Uri("/api/v1/sessions", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Forbidden, blocked.StatusCode);
        Assert.Equal("password_change_required", await blocked.ErrorCode());

        var tooShort = await c.PostAsJsonAsync("/api/v1/auth/change-password",
            new { currentPassword = ApiFactory.TestPassword, newPassword = "short" });
        Assert.Equal(HttpStatusCode.BadRequest, tooShort.StatusCode);
        Assert.Equal("password_too_short", (await tooShort.Content.ReadFromJsonAsync<ErrorBody>())!.Fields!["newPassword"]);

        var wrongCurrent = await c.PostAsJsonAsync("/api/v1/auth/change-password",
            new { currentPassword = "not-the-password", newPassword = "Brand-New-Pass-1" });
        Assert.Equal("wrong_password", (await wrongCurrent.Content.ReadFromJsonAsync<ErrorBody>())!.Fields!["currentPassword"]);

        var ok = await c.PostAsJsonAsync("/api/v1/auth/change-password",
            new { currentPassword = ApiFactory.TestPassword, newPassword = "Brand-New-Pass-1" });
        ok.EnsureSuccessStatusCode();
        var changed = await ok.Content.ReadFromJsonAsync<LoginBody>();
        Assert.False(changed!.User!.MustChangePassword);

        c.WithToken(changed.AccessToken);
        Assert.Equal(HttpStatusCode.OK, (await c.GetAsync(new Uri("/api/v1/sessions", UriKind.Relative))).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await _api.Native().Login(t.Code, "first1", ApiFactory.TestPassword)).StatusCode);
        Assert.Equal("ok", (await _api.Native().LoginOk(t.Code, "first1", "Brand-New-Pass-1")).Stage);
        Assert.Contains(await AuditFor(t.Id), a => a.Action == "password_change" && a.Result == AuditResult.Success);
    }

    // ---------------- BR-AUD-03 ----------------

    [Fact]
    public async Task test_BR_AUD_03_audit_log_cannot_be_updated_or_deleted()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "aud1", UserRole.Guard);
        await _api.CreateClient().Login(t.Code, "aud1", "wrong-password");
        var id = (await AuditFor(t.Id)).First().Id;

        await _api.WithDb(async db =>
        {
            await Assert.ThrowsAsync<PostgresException>(() =>
                db.Database.ExecuteSqlInterpolatedAsync($"UPDATE audit_logs SET result = 'success' WHERE id = {id}"));
            await Assert.ThrowsAsync<PostgresException>(() =>
                db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM audit_logs WHERE id = {id}"));
            await Assert.ThrowsAsync<PostgresException>(() =>
                db.Database.ExecuteSqlRawAsync("TRUNCATE audit_logs"));
        });

        Assert.Equal(AuditResult.Failure, (await AuditFor(t.Id)).First(a => a.Id == id).Result);
    }

    [Fact]
    public async Task test_BR_AUD_01_successful_login_is_audited()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "aud2", UserRole.Manager);
        await _api.Native().LoginOk(t.Code, "aud2");

        var entry = Assert.Single(await AuditFor(t.Id), a => a.Action == "login" && a.Result == AuditResult.Success);
        Assert.Equal("aud2", entry.UserLogin);
        Assert.Equal("manager", entry.Role);
        Assert.Equal(AuditCategory.Auth, entry.Category);
    }

    private sealed record SessionBody(Guid Id, string Login, string Device, string Ip, bool IsCurrent);

    private sealed record SetupBody(string Secret, string OtpauthUri);
}
