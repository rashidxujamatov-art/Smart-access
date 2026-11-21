using System.Globalization;
using SmartAccess.Application.Abstractions;
using SmartAccess.Application.Common;
using SmartAccess.Infrastructure.Security;

namespace SmartAccess.WebApi.Infrastructure;

/// <summary>Cheklangan token bilan ham ochiq bo'lgan endpointlar (parolni almashtirish, 2FA sozlash, me, logout).</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class AllowRestrictedAttribute : Attribute;

/// <summary>
/// BR-USR-02 (birinchi kirishda parolni almashtirish) va security.require_2fa_for_admin:
/// shart bajarilmaguncha boshqa barcha endpointlar 403 qaytaradi.
/// </summary>
public sealed class AccessRestrictionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var claim = ctx.User.FindFirst(ClaimNames.Restriction)?.Value;
        if (claim is not null
            && ctx.GetEndpoint()?.Metadata.GetMetadata<AllowRestrictedAttribute>() is null)
        {
            var r = (AccessRestriction)int.Parse(claim, CultureInfo.InvariantCulture);
            ctx.Response.StatusCode = 403;
            await ctx.Response.WriteAsJsonAsync(new ApiError(r.HasFlag(AccessRestriction.MustChangePassword)
                ? ErrorCodes.PasswordChangeRequired
                : ErrorCodes.TwoFactorSetupRequired));
            return;
        }

        await next(ctx);
    }
}
