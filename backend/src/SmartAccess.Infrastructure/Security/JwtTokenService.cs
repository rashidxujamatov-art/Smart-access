using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartAccess.Application.Abstractions;
using SmartAccess.Application.Features.Auth;
using SmartAccess.Domain.Users;

namespace SmartAccess.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string Issuer = "smart-access";
    public const string AccessAudience = "smart-access-api";
    public const string ChallengeAudience = "smart-access-2fa";

    /// <summary>JWT_SIGNING_KEY — kamida 64 belgi, faqat muhit o'zgaruvchisida (BR-PRIV-06).</summary>
    public required string SigningKey { get; init; }

    public SymmetricSecurityKey Key() => new(Encoding.UTF8.GetBytes(SigningKey));
}

public static class ClaimNames
{
    public const string Tenant = "tid";
    public const string Session = "sid";
    public const string Role = "role";
    public const string Login = "login";
    public const string Restriction = "rst";
    public const string RememberMe = "rem";
}

/// <summary>BR-AUTH-05: access JWT (15 daqiqa) + tasodifiy refresh token (bazada faqat SHA-256 xeshi).</summary>
public sealed class JwtTokenService(JwtOptions jwt, IOptions<AuthOptions> auth, TimeProvider time) : ITokenService
{
    private readonly JwtSecurityTokenHandler _handler = new() { MapInboundClaims = false };

    public AccessToken CreateAccessToken(User user, Session session, AccessRestriction restriction)
    {
        var now = time.GetUtcNow();
        var expires = now.AddMinutes(auth.Value.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimNames.Tenant, user.TenantId.ToString()),
            new(ClaimNames.Session, session.Id.ToString()),
            new(ClaimNames.Role, AuthService.RoleCode(user.Role)),
            new(ClaimNames.Login, user.Login),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        if (restriction != AccessRestriction.None)
        {
            claims.Add(new Claim(ClaimNames.Restriction, ((int)restriction).ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        return new AccessToken(Write(claims, JwtOptions.AccessAudience, now, expires), expires);
    }

    public string CreateChallengeToken(Guid userId, Guid tenantId, bool rememberMe)
    {
        var now = time.GetUtcNow();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimNames.Tenant, tenantId.ToString()),
            new(ClaimNames.RememberMe, rememberMe ? "1" : "0"),
        };
        return Write(claims, JwtOptions.ChallengeAudience, now, now.AddMinutes(auth.Value.ChallengeMinutes));
    }

    public ChallengeClaims? ReadChallengeToken(string token)
    {
        try
        {
            var principal = _handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidIssuer = JwtOptions.Issuer,
                ValidAudience = JwtOptions.ChallengeAudience,
                IssuerSigningKey = jwt.Key(),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ClockSkew = TimeSpan.Zero,
                LifetimeValidator = (nb, exp, _, _) => exp is { } e && e > time.GetUtcNow().UtcDateTime,
            }, out _);
            return new ChallengeClaims(
                Guid.Parse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!),
                Guid.Parse(principal.FindFirst(ClaimNames.Tenant)?.Value!),
                principal.FindFirst(ClaimNames.RememberMe)?.Value == "1");
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return null;
        }
    }

    public string NewRefreshToken() => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));

    public string HashRefreshToken(string refreshToken) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

    private string Write(IEnumerable<Claim> claims, string audience, DateTimeOffset now, DateTimeOffset expires)
    {
        var token = new JwtSecurityToken(
            issuer: JwtOptions.Issuer,
            audience: audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(jwt.Key(), SecurityAlgorithms.HmacSha256));
        return _handler.WriteToken(token);
    }
}
