using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using OtpNet;
using SmartAccess.Application.Abstractions;
using SmartAccess.Domain.Settings;
using SmartAccess.Domain.Tenants;
using SmartAccess.Domain.Users;
using SmartAccess.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SmartAccess.Tests.Infrastructure;

/// <summary>Bitta PostgreSQL konteyneri barcha integratsion testlar uchun.</summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16.10-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition(Name)]
public sealed class DbCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "db";
}

/// <summary>Haqiqiy API + haqiqiy baza. Vaqt — FakeTimeProvider (blok va muddatlarni sinash uchun).</summary>
public sealed class ApiFactory(PostgresFixture pg) : WebApplicationFactory<Program>
{
    public const string TestPassword = "Correct-Horse-1";

    public FakeTimeProvider Time { get; } = new(DateTimeOffset.UtcNow);

    public byte[] EncryptionKey { get; } = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", pg.ConnectionString);
        builder.UseSetting("JWT_SIGNING_KEY", new string('k', 64));
        builder.UseSetting("ENCRYPTION_KEY_BASE64", Convert.ToBase64String(EncryptionKey));
        builder.ConfigureTestServices(s =>
        {
            s.AddSingleton<TimeProvider>(Time);
        });
    }

    /// <summary>Native klient: refresh token javob tanasida keladi.</summary>
    public HttpClient Native()
    {
        var c = CreateClient();
        c.DefaultRequestHeaders.Add("X-Client", "native");
        c.DefaultRequestHeaders.UserAgent.ParseAdd("SmartAccessTests/1.0");
        return c;
    }

    public async Task<T> WithDb<T>(Func<AppDbContext, IServiceProvider, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<AppDbContext>(), scope.ServiceProvider);
    }

    public Task WithDb(Func<AppDbContext, Task> action) =>
        WithDb<int>(async (db, _) =>
        {
            await action(db);
            return 0;
        });

    /// <summary>Har test o'z korxonasini oladi — testlar bir-biriga ta'sir qilmaydi.</summary>
    public Task<Tenant> CreateTenantAsync(bool require2faForAdmin = false) =>
        WithDb(async (db, _) =>
        {
            var t = new Tenant { Code = "T-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(), Name = "Test" };
            db.Tenants.Add(t);
            db.Settings.Add(new Setting
            {
                TenantId = t.Id,
                Key = SettingKeys.SecurityRequire2faForAdmin,
                Value = require2faForAdmin ? "true" : "false",
            });
            await db.SaveChangesAsync();
            return t;
        });

    public Task<User> CreateUserAsync(Tenant tenant, string login, UserRole role, bool mustChange = false,
        byte[]? totpSecret = null, UserStatus status = UserStatus.Active) =>
        WithDb(async (db, sp) =>
        {
            var u = new User
            {
                TenantId = tenant.Id,
                Login = login,
                FullName = "Test " + login,
                Role = role,
                PasswordHash = sp.GetRequiredService<IPasswordHasher>().Hash(TestPassword),
                MustChangePassword = mustChange,
                Status = status,
                TotpSecretEnc = totpSecret is null ? null : sp.GetRequiredService<ISecretProtector>().Protect(totpSecret),
            };
            db.Users.Add(u);
            await db.SaveChangesAsync();
            return u;
        });

    public string TotpNow(byte[] secret) => new Totp(secret).ComputeTotp(Time.GetUtcNow().UtcDateTime);
}

public static class HttpExtensions
{
    public static Task<HttpResponseMessage> Login(this HttpClient c, string tenant, string login, string password,
        bool remember = true) =>
        c.PostAsJsonAsync("/api/v1/auth/login", new { tenantCode = tenant, login, password, rememberMe = remember });

    public static async Task<LoginBody> LoginOk(this HttpClient c, string tenant, string login,
        string password = ApiFactory.TestPassword, bool remember = true)
    {
        var r = await c.Login(tenant, login, password, remember);
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<LoginBody>())!;
    }

    public static HttpClient WithToken(this HttpClient c, string? token)
    {
        c.DefaultRequestHeaders.Authorization = token is null ? null : new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    public static async Task<string?> ErrorCode(this HttpResponseMessage r) =>
        (await r.Content.ReadFromJsonAsync<ErrorBody>())?.Code;
}

public sealed record LoginBody(
    string Stage,
    string? AccessToken,
    DateTimeOffset? AccessTokenExpiresAt,
    UserBody? User,
    string? ChallengeToken,
    string? RefreshToken,
    DateTimeOffset? RefreshTokenExpiresAt);

public sealed record UserBody(string Login, string FullName, string Role, string Home, bool TwoFactorEnabled,
    bool MustChangePassword, bool MustSetup2fa);

public sealed record ErrorBody(string Code, Dictionary<string, string>? Fields);
