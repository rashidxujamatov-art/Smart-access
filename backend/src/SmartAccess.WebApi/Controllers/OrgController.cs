using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAccess.Application.Features.Org;

namespace SmartAccess.WebApi.Controllers;

/// <summary>
/// Diqqat: klass va metod [Authorize] atributlari birga (VA) tekshiriladi,
/// shuning uchun rollar faqat metodlarda.
/// CLAUDE.md 7-bo'lim — Org. CRUD faqat administrator (3-bo'lim).
/// O'qish administrator va mas'ul shaxs uchun: davomat va hisobotlardagi bo'lim/guruh filtri (4.5.4, BR-REP-01).
/// </summary>
[ApiController]
[Route("api/v1/departments")]
[Authorize]
public sealed class DepartmentsController(OrgService org) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.AdminOrManager)]
    public Task<IReadOnlyList<DepartmentDto>> List(CancellationToken ct) => org.ListDepartmentsAsync(ct);

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(CreateDepartmentRequest req, CancellationToken ct) =>
        StatusCode(201, await org.CreateDepartmentAsync(req, ct));

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public Task<DepartmentDto> Update(Guid id, UpdateDepartmentRequest req, CancellationToken ct) =>
        org.UpdateDepartmentAsync(id, req, ct);

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await org.DeleteDepartmentAsync(id, ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/v1/groups")]
[Authorize]
public sealed class GroupsController(OrgService org) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.AdminOrManager)]
    public Task<IReadOnlyList<GroupDto>> List([FromQuery] Guid? department, CancellationToken ct) =>
        org.ListGroupsAsync(department, ct);

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(CreateGroupRequest req, CancellationToken ct) =>
        StatusCode(201, await org.CreateGroupAsync(req, ct));

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public Task<GroupDto> Update(Guid id, UpdateGroupRequest req, CancellationToken ct) =>
        org.UpdateGroupAsync(id, req, ct);

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await org.DeleteGroupAsync(id, ct);
        return NoContent();
    }
}

/// <summary>Rol nomlari (JWT "role" claim) — CLAUDE.md 3-bo'lim.</summary>
public static class Roles
{
    public const string Guard = "guard";
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string AdminOrManager = "admin,manager";
    public const string GuardOrAdmin = "guard,admin";
}
