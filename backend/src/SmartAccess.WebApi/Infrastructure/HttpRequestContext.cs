using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SmartAccess.Application.Abstractions;
using SmartAccess.Domain.Users;
using SmartAccess.Infrastructure.Security;

namespace SmartAccess.WebApi.Infrastructure;

/// <summary>So'rovdan IP, qurilma (User-Agent) va JWT'dagi foydalanuvchini o'qiydi.</summary>
public sealed class HttpRequestContext(IHttpContextAccessor accessor) : IRequestContext, ITenantProvider
{
    private const int MaxDeviceLength = 256;

    private HttpContext? Http => accessor.HttpContext;

    public string Ip => Http?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    public string Device
    {
        get
        {
            var ua = Http?.Request.Headers.UserAgent.ToString();
            if (string.IsNullOrWhiteSpace(ua))
            {
                return "unknown";
            }

            return ua.Length > MaxDeviceLength ? ua[..MaxDeviceLength] : ua;
        }
    }

    public CurrentUser? User => FromPrincipal(Http?.User);

    public Guid? TenantId => User?.TenantId;

    public static CurrentUser? FromPrincipal(ClaimsPrincipal? p)
    {
        if (p?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        if (!Guid.TryParse(p.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId)
            || !Guid.TryParse(p.FindFirstValue(ClaimNames.Tenant), out var tenantId)
            || !Guid.TryParse(p.FindFirstValue(ClaimNames.Session), out var sessionId)
            || !Enum.TryParse<UserRole>(p.FindFirstValue(ClaimNames.Role), true, out var role))
        {
            return null;
        }

        return new CurrentUser(userId, tenantId, sessionId, role, p.FindFirstValue(ClaimNames.Login) ?? "");
    }
}
