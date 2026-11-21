using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartAccess.Application;
using SmartAccess.Application.Abstractions;
using SmartAccess.Application.Features.Auth;
using SmartAccess.Infrastructure;
using SmartAccess.Infrastructure.Persistence;
using SmartAccess.Infrastructure.Security;
using SmartAccess.WebApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpRequestContext>();
builder.Services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<HttpRequestContext>());
builder.Services.AddScoped<ITenantProvider>(sp => sp.GetRequiredService<HttpRequestContext>());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(config);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuditingAuthorizationResultHandler>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<JwtOptions>((o, jwt) =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = JwtOptions.Issuer,
            ValidAudience = JwtOptions.AccessAudience,
            IssuerSigningKey = jwt.Key(),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimNames.Login,
            RoleClaimType = ClaimNames.Role,
        };
        o.Events = new JwtBearerEvents
        {
            // BR-AUTH-07: tugatilgan sessiya tokeni darhol yaroqsiz.
            OnTokenValidated = async ctx =>
            {
                var user = HttpRequestContext.FromPrincipal(ctx.Principal);
                var sessions = ctx.HttpContext.RequestServices.GetRequiredService<SessionService>();
                if (user is null || !await sessions.IsSessionValidAsync(user.SessionId, user.UserId, ctx.HttpContext.RequestAborted))
                {
                    ctx.Fail("session_revoked");
                }
            },
        };
    });
builder.Services.AddAuthorization();

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    // Caddy orqasida haqiqiy mijoz IP'si (audit uchun). Faqat ishonchli proksi bo'lsa yoqiladi.
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    if (config.GetValue("TRUST_FORWARDED_HEADERS", false))
    {
        o.KnownIPNetworks.Clear();
        o.KnownProxies.Clear();
    }
});

var app = builder.Build();

// --- Bir martalik buyruqlar (seed alohida — CLAUDE.md 0-bo'lim, 7-qoida) ---
if (args.Length > 0 && args[0] is "seed-demo" or "bootstrap" or "migrate")
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    switch (args[0])
    {
        case "seed-demo":
            await seeder.SeedDemoAsync();
            Console.WriteLine($"Demo ma'lumot tayyor: korxona {DatabaseSeeder.DemoTenantCode}, parol '{DatabaseSeeder.DemoPassword}'.");
            break;
        case "bootstrap":
            string Env(string k) => config[k] ?? throw new InvalidOperationException($"{k} o'rnatilmagan.");
            await seeder.BootstrapAsync(Env("BOOTSTRAP_TENANT_CODE"), Env("BOOTSTRAP_TENANT_NAME"),
                Env("BOOTSTRAP_ADMIN_LOGIN"), Env("BOOTSTRAP_ADMIN_FULL_NAME"), Env("BOOTSTRAP_ADMIN_PASSWORD"));
            Console.WriteLine("Korxona va administrator yaratildi. Birinchi kirishda parolni almashtirish majburiy.");
            break;
    }

    return;
}

if (config.GetValue("MIGRATE_ON_STARTUP", true))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseForwardedHeaders();
app.UseMiddleware<AppExceptionMiddleware>();
app.UseAuthentication();
app.UseMiddleware<AccessRestrictionMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/v1/health");

app.Run();

/// <summary>Integratsion testlar (WebApplicationFactory) uchun ochiq kirish nuqtasi.</summary>
public partial class Program;
