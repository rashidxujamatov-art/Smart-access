using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAccess.Application.Abstractions;
using SmartAccess.Application.Common;
using SmartAccess.Application.Features.Auth;
using SmartAccess.WebApi.Infrastructure;

namespace SmartAccess.WebApi.Controllers;

/// <summary>CLAUDE.md 7-bo'lim — Auth. Ro'yxatdan o'tish endpointi YO'Q (BR-AUTH-01).</summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(AuthService auth, IRequestContext request) : ControllerBase
{
    public const string RefreshCookie = "sa_refresh";

    /// <summary>Desktop/mobil klient refresh tokenni cookie o'rniga javob tanasida oladi (xavfsiz saqlash — BR-MOB-06).</summary>
    public const string NativeClientHeader = "X-Client";

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest req, CancellationToken ct) =>
        Respond(await auth.LoginAsync(req, ct));

    [HttpPost("2fa")]
    [AllowAnonymous]
    public async Task<IActionResult> TwoFactor(TwoFactorRequest req, CancellationToken ct) =>
        Respond(await auth.VerifyTwoFactorAsync(req, ct));

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest? req, CancellationToken ct)
    {
        var token = Request.Cookies[RefreshCookie] ?? req?.RefreshToken;
        try
        {
            return Respond(await auth.RefreshAsync(token, ct));
        }
        catch (AppException)
        {
            Response.Cookies.Delete(RefreshCookie, CookieOptions(null));
            throw;
        }
    }

    [HttpPost("logout")]
    [Authorize]
    [AllowRestricted]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await auth.LogoutAsync(request.User!, ct);
        Response.Cookies.Delete(RefreshCookie, CookieOptions(null));
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [AllowRestricted]
    public async Task<UserInfo> Me(CancellationToken ct) => await auth.MeAsync(request.User!, ct);

    [HttpPost("change-password")]
    [Authorize]
    [AllowRestricted]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest req, CancellationToken ct) =>
        Respond(await auth.ChangePasswordAsync(request.User!, req, ct));

    /// <summary>2FA ni yoqish, 1-qadam: sir va otpauth URI (QR kod uchun). 13-bo'lim, 12-savol.</summary>
    [HttpPost("2fa/setup")]
    [Authorize]
    [AllowRestricted]
    public Task<TwoFactorSetup> SetupTwoFactor(CancellationToken ct) =>
        auth.BeginTwoFactorSetupAsync(request.User!, ct);

    /// <summary>2FA ni yoqish, 2-qadam: ilovadagi kod bilan tasdiqlash.</summary>
    [HttpPost("2fa/enable")]
    [Authorize]
    [AllowRestricted]
    public async Task<IActionResult> EnableTwoFactor(Enable2faRequest req, CancellationToken ct) =>
        Respond(await auth.EnableTwoFactorAsync(request.User!, req, ct));

    private IActionResult Respond(AuthResult r)
    {
        var native = Request.Headers[NativeClientHeader].ToString() == "native";
        if (r.RefreshToken is not null && !native)
        {
            Response.Cookies.Append(RefreshCookie, r.RefreshToken, CookieOptions(r.RefreshTokenExpiresAt));
        }

        return Ok(new LoginResponse(
            r.Stage,
            r.AccessToken,
            r.AccessTokenExpiresAt,
            r.User,
            r.ChallengeToken,
            native ? r.RefreshToken : null,
            r.RefreshTokenExpiresAt));
    }

    private CookieOptions CookieOptions(DateTimeOffset? expires) => new()
    {
        HttpOnly = true,
        Secure = Request.IsHttps || !HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Path = "/api/v1/auth",
        Expires = expires,
    };
}

public sealed record RefreshRequest(string? RefreshToken);

public sealed record LoginResponse(
    string Stage,
    string? AccessToken,
    DateTimeOffset? AccessTokenExpiresAt,
    UserInfo? User,
    string? ChallengeToken,
    string? RefreshToken,
    DateTimeOffset? RefreshTokenExpiresAt);
