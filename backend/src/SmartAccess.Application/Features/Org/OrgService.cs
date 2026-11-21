using Microsoft.EntityFrameworkCore;
using SmartAccess.Application.Abstractions;
using SmartAccess.Application.Audit;
using SmartAccess.Application.Common;
using SmartAccess.Domain.Audit;
using SmartAccess.Domain.Org;

namespace SmartAccess.Application.Features.Org;

public sealed record DepartmentDto(Guid Id, string Name, int GroupCount);

public sealed record GroupDto(Guid Id, string Name, Guid DepartmentId, string DepartmentName);

public sealed record CreateDepartmentRequest(string? Name);

public sealed record UpdateDepartmentRequest(string? Name);

public sealed record CreateGroupRequest(Guid? DepartmentId, string? Name);

public sealed record UpdateGroupRequest(string? Name, Guid? DepartmentId);

/// <summary>CLAUDE.md 4.3 — BR-ORG-01, BR-ORG-04, BR-ORG-05; audit — BR-AUD-01.</summary>
public sealed class OrgService(IAppDbContext db, IAuditLogger audit, IRequestContext request, TimeProvider time)
{
    public const int NameMaxLength = 100;

    // ---------------- Bo'limlar ----------------

    public async Task<IReadOnlyList<DepartmentDto>> ListDepartmentsAsync(CancellationToken ct) =>
        await db.Departments
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto(d.Id, d.Name, db.Groups.Count(g => g.DepartmentId == d.Id)))
            .ToListAsync(ct);

    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest req, CancellationToken ct)
    {
        var name = CleanName(req.Name);
        await EnsureDepartmentNameFreeAsync(name, null, ct);

        var dep = new Department { TenantId = TenantId, Name = name };
        db.Departments.Add(dep);
        await db.SaveChangesAsync(ct);
        await Audit("department_create", dep.Id, null, new { name }, ct);
        return new DepartmentDto(dep.Id, dep.Name, 0);
    }

    public async Task<DepartmentDto> UpdateDepartmentAsync(Guid id, UpdateDepartmentRequest req, CancellationToken ct)
    {
        var dep = await db.Departments.FirstOrDefaultAsync(d => d.Id == id, ct) ?? throw AppException.NotFound();
        var name = CleanName(req.Name);
        await EnsureDepartmentNameFreeAsync(name, id, ct);

        var old = dep.Name;
        dep.Name = name;
        await db.SaveChangesAsync(ct);
        await Audit("department_update", dep.Id, new { name = old }, new { name }, ct);
        return new DepartmentDto(dep.Id, dep.Name, await db.Groups.CountAsync(g => g.DepartmentId == id, ct));
    }

    /// <summary>BR-ORG-04: ichida guruh bor bo'limni o'chirish taqiqlanadi.</summary>
    public async Task DeleteDepartmentAsync(Guid id, CancellationToken ct)
    {
        var dep = await db.Departments.FirstOrDefaultAsync(d => d.Id == id, ct) ?? throw AppException.NotFound();
        var groups = await db.Groups.CountAsync(g => g.DepartmentId == id, ct);
        if (groups > 0)
        {
            await Audit("department_delete", dep.Id, new { name = dep.Name }, null, ct, AuditResult.Denied);
            throw AppException.Conflict(ErrorCodes.DepartmentHasGroups, new { groups });
        }

        dep.DeletedAt = time.GetUtcNow();
        await db.SaveChangesAsync(ct);
        await Audit("department_delete", dep.Id, new { name = dep.Name }, null, ct);
    }

    // ---------------- Guruhlar ----------------

    public async Task<IReadOnlyList<GroupDto>> ListGroupsAsync(Guid? departmentId, CancellationToken ct) =>
        await db.Groups
            .Where(g => departmentId == null || g.DepartmentId == departmentId)
            .OrderBy(g => g.Department!.Name).ThenBy(g => g.Name)
            .Select(g => new GroupDto(g.Id, g.Name, g.DepartmentId, g.Department!.Name))
            .ToListAsync(ct);

    /// <summary>BR-ORG-01: guruh albatta bitta bo'limga tegishli.</summary>
    public async Task<GroupDto> CreateGroupAsync(CreateGroupRequest req, CancellationToken ct)
    {
        var name = CleanName(req.Name);
        var dep = await RequireDepartmentAsync(req.DepartmentId, ct);
        await EnsureGroupNameFreeAsync(dep.Id, name, null, ct);

        var group = new Group { TenantId = TenantId, DepartmentId = dep.Id, Name = name };
        db.Groups.Add(group);
        await db.SaveChangesAsync(ct);
        await Audit("group_create", group.Id, null, new { name, department = dep.Name }, ct);
        return new GroupDto(group.Id, group.Name, dep.Id, dep.Name);
    }

    public async Task<GroupDto> UpdateGroupAsync(Guid id, UpdateGroupRequest req, CancellationToken ct)
    {
        var group = await db.Groups.Include(g => g.Department).FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw AppException.NotFound();

        var name = req.Name is null ? group.Name : CleanName(req.Name);
        var dep = req.DepartmentId is null ? group.Department! : await RequireDepartmentAsync(req.DepartmentId, ct);
        await EnsureGroupNameFreeAsync(dep.Id, name, id, ct);

        var old = new { name = group.Name, department = group.Department!.Name };
        group.Name = name;
        group.DepartmentId = dep.Id;
        await db.SaveChangesAsync(ct);
        await Audit("group_update", group.Id, old, new { name, department = dep.Name }, ct);
        return new GroupDto(group.Id, group.Name, dep.Id, dep.Name);
    }

    /// <summary>
    /// BR-ORG-04: ichida xodim bor guruhni o'chirish taqiqlanadi. Xodimlar jadvali 4-bandda (EMP) qo'shiladi —
    /// o'sha bandda bu yerga xodimlar soni tekshiruvi ulanadi (docs/progress.md, 3-band).
    /// </summary>
    public async Task DeleteGroupAsync(Guid id, CancellationToken ct)
    {
        var group = await db.Groups.Include(g => g.Department).FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw AppException.NotFound();

        group.DeletedAt = time.GetUtcNow();
        await db.SaveChangesAsync(ct);
        await Audit("group_delete", group.Id, new { name = group.Name, department = group.Department!.Name }, null, ct);
    }

    // ---------------- Yordamchilar ----------------

    private Guid TenantId => request.User?.TenantId ?? throw AppException.Unauthorized();

    private static string CleanName(string? name)
    {
        var n = (name ?? "").Trim();
        if (n.Length == 0)
        {
            throw AppException.Validation("name", ErrorCodes.Required);
        }

        if (n.Length > NameMaxLength)
        {
            throw AppException.Validation("name", ErrorCodes.TooLong);
        }

        return n;
    }

    private async Task<Department> RequireDepartmentAsync(Guid? id, CancellationToken ct)
    {
        if (id is null)
        {
            throw AppException.Validation("departmentId", ErrorCodes.Required);
        }

        // Tenant filtri: boshqa korxonaning bo'limi topilmaydi.
        return await db.Departments.FirstOrDefaultAsync(d => d.Id == id, ct)
            ?? throw AppException.Validation("departmentId", ErrorCodes.DepartmentNotFound);
    }

    /// <summary>BR-ORG-05 (bo'limlar uchun — korxona ichida unikal), katta-kichik harfsiz.</summary>
    private async Task EnsureDepartmentNameFreeAsync(string name, Guid? exceptId, CancellationToken ct)
    {
        var lower = name.ToLowerInvariant();
        if (await db.Departments.AnyAsync(d => d.Id != exceptId && d.Name.ToLower() == lower, ct))
        {
            throw AppException.Validation("name", ErrorCodes.NameTaken);
        }
    }

    /// <summary>BR-ORG-05: guruh nomi bo'lim ichida unikal.</summary>
    private async Task EnsureGroupNameFreeAsync(Guid departmentId, string name, Guid? exceptId, CancellationToken ct)
    {
        var lower = name.ToLowerInvariant();
        if (await db.Groups.AnyAsync(g => g.DepartmentId == departmentId && g.Id != exceptId && g.Name.ToLower() == lower, ct))
        {
            throw AppException.Validation("name", ErrorCodes.NameTaken);
        }
    }

    private Task Audit(string action, Guid id, object? oldValue, object? newValue, CancellationToken ct,
        string result = AuditResult.Success) =>
        audit.WriteAsync(new AuditEntry(AuditCategory.Data, action, result,
            action.StartsWith("department", StringComparison.Ordinal) ? "department" : "group", id.ToString(),
            oldValue, newValue), ct: ct);
}
