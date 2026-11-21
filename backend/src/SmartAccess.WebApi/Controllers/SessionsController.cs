using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAccess.Application.Features.Auth;

namespace SmartAccess.WebApi.Controllers;

/// <summary>BR-AUTH-07: faol sessiyalar — faqat administrator (CLAUDE.md 3-bo'lim).</summary>
[ApiController]
[Route("api/v1/sessions")]
[Authorize(Roles = "admin")]
public sealed class SessionsController(SessionService sessions) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<SessionInfo>> List(CancellationToken ct) => sessions.ListActiveAsync(ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Terminate(Guid id, CancellationToken ct)
    {
        await sessions.TerminateAsync(id, ct);
        return NoContent();
    }
}
