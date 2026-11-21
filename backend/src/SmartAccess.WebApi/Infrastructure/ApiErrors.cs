using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using SmartAccess.Application.Audit;
using SmartAccess.Application.Common;
using SmartAccess.Domain.Audit;

namespace SmartAccess.WebApi.Infrastructure;

/// <summary>Yagona xato formati: {"code": "...", "fields": {...}, "details": {...}}.</summary>
public sealed record ApiError(string Code, IReadOnlyDictionary<string, string>? Fields = null, object? Details = null);

public sealed class AppExceptionMiddleware(RequestDelegate next, ILogger<AppExceptionMiddleware> log)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (AppException ex) when (!ctx.Response.HasStarted)
        {
            ctx.Response.StatusCode = ex.Status;
            await ctx.Response.WriteAsJsonAsync(new ApiError(ex.Code, ex.Fields, ex.Details));
        }
        catch (Exception ex) when (!ctx.Response.HasStarted && ex is not OperationCanceledException)
        {
            log.LogError(ex, "Kutilmagan xato: {Path}", ctx.Request.Path);
            ctx.Response.StatusCode = 500;
            await ctx.Response.WriteAsJsonAsync(new ApiError("internal_error"));
        }
    }
}

/// <summary>
/// BR-RBAC-01/04: ruxsatsiz urinish 403 qaytaradi va audit logga yoziladi; kirilmagan bo'lsa 401.
/// </summary>
public sealed class AuditingAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    public async Task HandleAsync(RequestDelegate next, HttpContext ctx, AuthorizationPolicy policy, PolicyAuthorizationResult result)
    {
        if (result.Challenged)
        {
            ctx.Response.StatusCode = 401;
            await ctx.Response.WriteAsJsonAsync(new ApiError(ErrorCodes.Unauthorized));
            return;
        }

        if (result.Forbidden)
        {
            var audit = ctx.RequestServices.GetRequiredService<IAuditLogger>();
            await audit.WriteAsync(new AuditEntry(AuditCategory.Access, "forbidden", AuditResult.Denied,
                NewValue: new { method = ctx.Request.Method, path = ctx.Request.Path.Value }), ct: ctx.RequestAborted);
            ctx.Response.StatusCode = 403;
            await ctx.Response.WriteAsJsonAsync(new ApiError(ErrorCodes.Forbidden));
            return;
        }

        await next(ctx);
    }
}
