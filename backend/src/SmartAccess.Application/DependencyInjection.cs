using Microsoft.Extensions.DependencyInjection;
using SmartAccess.Application.Audit;
using SmartAccess.Application.Features.Auth;
using SmartAccess.Application.Settings;

namespace SmartAccess.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<SettingsReader>();
        services.AddScoped<AuthService>();
        services.AddScoped<SessionService>();
        return services;
    }
}
