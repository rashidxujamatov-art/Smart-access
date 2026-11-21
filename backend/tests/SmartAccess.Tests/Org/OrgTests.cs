using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SmartAccess.Domain.Audit;
using SmartAccess.Domain.Tenants;
using SmartAccess.Domain.Users;
using SmartAccess.Tests.Infrastructure;

namespace SmartAccess.Tests.Org;

/// <summary>CLAUDE.md 4.3 — BR-ORG-01, BR-ORG-04, BR-ORG-05; BR-RBAC-01/04; BR-AUD-01.</summary>
[Collection(DbCollection.Name)]
public sealed class OrgTests(PostgresFixture pg) : IDisposable
{
    private readonly ApiFactory _api = new(pg);

    public void Dispose() => _api.Dispose();

    private async Task<(Tenant Tenant, HttpClient Admin)> AdminAsync()
    {
        var t = await _api.CreateTenantAsync();
        await _api.CreateUserAsync(t, "adm", UserRole.Admin);
        var c = _api.Native();
        c.WithToken((await c.LoginOk(t.Code, "adm")).AccessToken);
        return (t, c);
    }

    private async Task<HttpClient> UserAsync(Tenant t, string login, UserRole role)
    {
        await _api.CreateUserAsync(t, login, role);
        var c = _api.Native();
        return c.WithToken((await c.LoginOk(t.Code, login)).AccessToken);
    }

    private static async Task<Dep> NewDepartment(HttpClient c, string name)
    {
        var r = await c.PostAsJsonAsync("/api/v1/departments", new { name });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        return (await r.Content.ReadFromJsonAsync<Dep>())!;
    }

    private static async Task<Grp> NewGroup(HttpClient c, Guid departmentId, string name)
    {
        var r = await c.PostAsJsonAsync("/api/v1/groups", new { departmentId, name });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        return (await r.Content.ReadFromJsonAsync<Grp>())!;
    }

    private static async Task<string?> FieldError(HttpResponseMessage r, string field) =>
        (await r.Content.ReadFromJsonAsync<ErrorBody>())?.Fields?.GetValueOrDefault(field);

    // ---------------- BR-ORG-01 ----------------

    [Fact]
    public async Task test_BR_ORG_01_group_belongs_to_exactly_one_department()
    {
        var (_, admin) = await AdminAsync();
        var dep = await NewDepartment(admin, "Ishlab chiqarish");
        var grp = await NewGroup(admin, dep.Id, "1-smena brigadasi");
        Assert.Equal(dep.Id, grp.DepartmentId);
        Assert.Equal("Ishlab chiqarish", grp.DepartmentName);

        var noDep = await admin.PostAsJsonAsync("/api/v1/groups", new { name = "Bo'limsiz" });
        Assert.Equal(HttpStatusCode.BadRequest, noDep.StatusCode);
        Assert.Equal("required", await FieldError(noDep, "departmentId"));

        var badDep = await admin.PostAsJsonAsync("/api/v1/groups", new { departmentId = Guid.NewGuid(), name = "X" });
        Assert.Equal("department_not_found", await FieldError(badDep, "departmentId"));

        var list = await admin.GetFromJsonAsync<List<Grp>>($"/api/v1/groups?department={dep.Id}");
        Assert.Single(list!);
        var deps = await admin.GetFromJsonAsync<List<Dep>>("/api/v1/departments");
        Assert.Equal(1, Assert.Single(deps!).GroupCount);
    }

    [Fact]
    public async Task test_BR_ORG_01_group_can_move_to_another_department()
    {
        var (_, admin) = await AdminAsync();
        var a = await NewDepartment(admin, "A");
        var b = await NewDepartment(admin, "B");
        var grp = await NewGroup(admin, a.Id, "Guruh");

        var r = await admin.PatchAsJsonAsync($"/api/v1/groups/{grp.Id}", new { departmentId = b.Id });
        r.EnsureSuccessStatusCode();
        var moved = await r.Content.ReadFromJsonAsync<Grp>();
        Assert.Equal(b.Id, moved!.DepartmentId);
        Assert.Equal("Guruh", moved.Name);
    }

    [Fact]
    public async Task test_BR_ORG_01_other_tenant_department_is_invisible()
    {
        var (_, adminA) = await AdminAsync();
        var (_, adminB) = await AdminAsync();
        var depB = await NewDepartment(adminB, "Ombor");

        Assert.Empty((await adminA.GetFromJsonAsync<List<Dep>>("/api/v1/departments"))!);
        var r = await adminA.PostAsJsonAsync("/api/v1/groups", new { departmentId = depB.Id, name = "Begona" });
        Assert.Equal("department_not_found", await FieldError(r, "departmentId"));
        Assert.Equal(HttpStatusCode.NotFound, (await adminA.DeleteAsync(new Uri($"/api/v1/departments/{depB.Id}", UriKind.Relative))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await adminA.PatchAsJsonAsync($"/api/v1/departments/{depB.Id}", new { name = "Buzish" })).StatusCode);
    }

    // ---------------- BR-ORG-04 ----------------

    [Fact]
    public async Task test_BR_ORG_04_department_with_groups_cannot_be_deleted()
    {
        var (t, admin) = await AdminAsync();
        var dep = await NewDepartment(admin, "Xavfsizlik");
        var grp = await NewGroup(admin, dep.Id, "Qorovullar");

        var blocked = await admin.DeleteAsync(new Uri($"/api/v1/departments/{dep.Id}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);
        Assert.Equal("department_has_groups", await blocked.ErrorCode());

        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync(new Uri($"/api/v1/groups/{grp.Id}", UriKind.Relative))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync(new Uri($"/api/v1/departments/{dep.Id}", UriKind.Relative))).StatusCode);
        Assert.Empty((await admin.GetFromJsonAsync<List<Dep>>("/api/v1/departments"))!);

        // Soft delete: yozuv bazada qoladi (tarixiy hisobotlar uchun).
        var stored = await _api.WithDb((db, _) => db.Departments.IgnoreQueryFilters().SingleAsync(d => d.Id == dep.Id));
        Assert.NotNull(stored.DeletedAt);
        Assert.Contains(await Audit(t), a => a.Action == "department_delete" && a.Result == AuditResult.Denied);
    }

    // ---------------- BR-ORG-05 ----------------

    [Fact]
    public async Task test_BR_ORG_05_group_name_unique_within_department_case_insensitive()
    {
        var (_, admin) = await AdminAsync();
        var a = await NewDepartment(admin, "Ombor");
        var b = await NewDepartment(admin, "Logistika");
        await NewGroup(admin, a.Id, "Kunduzgi");

        var dup = await admin.PostAsJsonAsync("/api/v1/groups", new { departmentId = a.Id, name = "  KUNDUZGI " });
        Assert.Equal(HttpStatusCode.BadRequest, dup.StatusCode);
        Assert.Equal("name_taken", await FieldError(dup, "name"));

        // Boshqa bo'limda bir xil nom — ruxsat.
        await NewGroup(admin, b.Id, "Kunduzgi");
    }

    [Fact]
    public async Task test_BR_ORG_05_department_name_unique_and_reusable_after_delete()
    {
        var (_, admin) = await AdminAsync();
        var dep = await NewDepartment(admin, "Ma'muriyat");

        var dup = await admin.PostAsJsonAsync("/api/v1/departments", new { name = "ma'muriyat" });
        Assert.Equal("name_taken", await FieldError(dup, "name"));

        var other = await NewDepartment(admin, "Boshqa");
        var rename = await admin.PatchAsJsonAsync($"/api/v1/departments/{other.Id}", new { name = "MA'MURIYAT" });
        Assert.Equal("name_taken", await FieldError(rename, "name"));

        await admin.DeleteAsync(new Uri($"/api/v1/departments/{dep.Id}", UriKind.Relative));
        await NewDepartment(admin, "Ma'muriyat");
    }

    [Theory]
    [InlineData("", "required")]
    [InlineData("   ", "required")]
    public async Task test_BR_ORG_05_name_is_required(string name, string code)
    {
        var (_, admin) = await AdminAsync();
        var r = await admin.PostAsJsonAsync("/api/v1/departments", new { name });
        Assert.Equal(code, await FieldError(r, "name"));
    }

    [Fact]
    public async Task test_BR_ORG_05_name_too_long_rejected()
    {
        var (_, admin) = await AdminAsync();
        var r = await admin.PostAsJsonAsync("/api/v1/departments", new { name = new string('a', 101) });
        Assert.Equal("too_long", await FieldError(r, "name"));
    }

    // ---------------- BR-AUD-01 ----------------

    [Fact]
    public async Task test_BR_AUD_01_org_changes_are_audited_with_old_and_new_values()
    {
        var (t, admin) = await AdminAsync();
        var dep = await NewDepartment(admin, "Eski nom");
        await admin.PatchAsJsonAsync($"/api/v1/departments/{dep.Id}", new { name = "Yangi nom" });
        var grp = await NewGroup(admin, dep.Id, "G1");
        await admin.DeleteAsync(new Uri($"/api/v1/groups/{grp.Id}", UriKind.Relative));

        var audit = await Audit(t);
        var update = Assert.Single(audit, a => a.Action == "department_update");
        Assert.Contains("Eski nom", update.OldValue!, StringComparison.Ordinal);
        Assert.Contains("Yangi nom", update.NewValue!, StringComparison.Ordinal);
        Assert.Equal(AuditCategory.Data, update.Category);
        Assert.Equal("adm", update.UserLogin);
        Assert.Contains(audit, a => a.Action == "department_create");
        Assert.Contains(audit, a => a.Action == "group_create");
        Assert.Contains(audit, a => a.Action == "group_delete" && a.ObjectId == grp.Id.ToString());
    }

    // ---------------- RBAC (CLAUDE.md 3-bo'lim) ----------------

    [Fact]
    public async Task test_BR_RBAC_01_manager_reads_but_cannot_modify_org()
    {
        var (t, admin) = await AdminAsync();
        var dep = await NewDepartment(admin, "D");
        var grp = await NewGroup(admin, dep.Id, "G");
        var manager = await UserAsync(t, "mgr", UserRole.Manager);

        Assert.Equal(HttpStatusCode.OK, (await manager.GetAsync(new Uri("/api/v1/departments", UriKind.Relative))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await manager.GetAsync(new Uri("/api/v1/groups", UriKind.Relative))).StatusCode);

        Assert.Equal(HttpStatusCode.Forbidden, (await manager.PostAsJsonAsync("/api/v1/departments", new { name = "X" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.PatchAsJsonAsync($"/api/v1/departments/{dep.Id}", new { name = "X" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.DeleteAsync(new Uri($"/api/v1/departments/{dep.Id}", UriKind.Relative))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.PostAsJsonAsync("/api/v1/groups", new { departmentId = dep.Id, name = "X" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.PatchAsJsonAsync($"/api/v1/groups/{grp.Id}", new { name = "X" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.DeleteAsync(new Uri($"/api/v1/groups/{grp.Id}", UriKind.Relative))).StatusCode);

        Assert.Equal(6, (await Audit(t)).Count(a => a.Action == "forbidden" && a.UserLogin == "mgr"));
    }

    [Fact]
    public async Task test_BR_RBAC_01_guard_has_no_access_to_org()
    {
        var (t, _) = await AdminAsync();
        var guard = await UserAsync(t, "grd", UserRole.Guard);

        Assert.Equal(HttpStatusCode.Forbidden, (await guard.GetAsync(new Uri("/api/v1/departments", UriKind.Relative))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await guard.GetAsync(new Uri("/api/v1/groups", UriKind.Relative))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await guard.PostAsJsonAsync("/api/v1/departments", new { name = "X" })).StatusCode);
        Assert.Equal(3, (await Audit(t)).Count(a => a.Action == "forbidden" && a.UserLogin == "grd"));
    }

    private Task<List<AuditLog>> Audit(Tenant t) =>
        _api.WithDb((db, _) => db.AuditLogs.IgnoreQueryFilters().Where(a => a.TenantId == t.Id).ToListAsync());

    private sealed record Dep(Guid Id, string Name, int GroupCount);

    private sealed record Grp(Guid Id, string Name, Guid DepartmentId, string DepartmentName);
}
