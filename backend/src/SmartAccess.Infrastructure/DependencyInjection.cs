using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartAccess.Application.Abstractions;
using SmartAccess.Application.Features.Auth;
using SmartAccess.Infrastructure.Persistence;
using SmartAccess.Infrastructure.Security;

namespace SmartAccess.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings__Default o'rnatilmagan.");

        services.AddDbContext<AppDbContext>(o => o
            .UseNpgsql(connectionString, n => n.MigrationsHistoryTable("__ef_migrations"))
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddOptions<AuthOptions>().Configure(o =>
        {
            o.AccessTokenMinutes = config.GetValue("JWT_ACCESS_MINUTES", o.AccessTokenMinutes);
            o.RefreshDays = config.GetValue("JWT_REFRESH_DAYS", o.RefreshDays);
        });

        var signingKey = config["JWT_SIGNING_KEY"];
        if (string.IsNullOrEmpty(signingKey) || signingKey.Length < 64)
        {
            throw new InvalidOperationException("JWT_SIGNING_KEY kamida 64 belgi bo'lishi kerak (.env).");
        }

        services.AddSingleton(new JwtOptions { SigningKey = signingKey });

        var encKey = config["ENCRYPTION_KEY_BASE64"];
        if (string.IsNullOrEmpty(encKey))
        {
            throw new InvalidOperationException("ENCRYPTION_KEY_BASE64 o'rnatilmagan (.env).");
        }

        services.AddSingleton<ISecretProtector>(new AesGcmSecretProtector(Convert.FromBase64String(encKey)));
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<ITotpService, TotpService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
