using Microsoft.EntityFrameworkCore;
using SmartAccess.Application.Abstractions;
using SmartAccess.Domain.Settings;
using SmartAccess.Domain.Tenants;
using SmartAccess.Domain.Users;

namespace SmartAccess.Infrastructure.Persistence;

/// <summary>
/// CLAUDE.md 0-bo'lim, 7-qoida: seed — migratsiyadan alohida.
/// Ishga tushirish: dotnet SmartAccess.WebApi.dll seed-demo | bootstrap
/// </summary>
public sealed class DatabaseSeeder(AppDbContext db, IPasswordHasher hasher)
{
    public const string DemoTenantCode = "KORXONA-001";
    public const string DemoPassword = "demo";

    /// <summary>DEMO_MODE uchun: dizayndagi demo korxona va hisoblar (parol: demo). Takror ishga tushirsa — o'zgartirmaydi.</summary>
    public async Task SeedDemoAsync(CancellationToken ct = default)
    {
        if (await db.Tenants.AnyAsync(t => t.Code == DemoTenantCode, ct))
        {
            return;
        }

        var tenant = new Tenant { Code = DemoTenantCode, Name = "Demo korxona" };
        db.Tenants.Add(tenant);

        var hash = hasher.Hash(DemoPassword);
        (string Login, string Name, UserRole Role)[] users =
        [
            ("admin", "Rustam Ochilov", UserRole.Admin),
            ("qorovul", "Farhod Ismoilov", UserRole.Guard),
            ("qorovul2", "Otabek Qodirov", UserRole.Guard),
            ("mas_ul", "Shahlo Karimova", UserRole.Manager),
        ];
        foreach (var (login, name, role) in users)
        {
            db.Users.Add(new User { TenantId = tenant.Id, Login = login, FullName = name, Role = role, PasswordHash = hash });
        }

        // Demo admin bir bosishda kirishi uchun 2FA majburiyati demo korxonada o'chiq (standart: true, CLAUDE.md 4.17).
        db.Settings.Add(new Setting { TenantId = tenant.Id, Key = SettingKeys.SecurityRequire2faForAdmin, Value = "false" });

        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Haqiqiy o'rnatish: birinchi korxona va administrator. Parol muhit o'zgaruvchisidan olinadi,
    /// birinchi kirishda almashtirish majburiy (BR-USR-02), 2FA majburiy (standart sozlama).
    /// </summary>
    public async Task BootstrapAsync(string tenantCode, string tenantName, string adminLogin, string adminFullName,
        string adminPassword, CancellationToken ct = default)
    {
        tenantCode = tenantCode.Trim().ToUpperInvariant();
        if (await db.Tenants.AnyAsync(t => t.Code == tenantCode, ct))
        {
            throw new InvalidOperationException($"Korxona '{tenantCode}' allaqachon mavjud.");
        }

        if (adminPassword.Length < 10)
        {
            throw new InvalidOperationException("Boshlang'ich parol kamida 10 belgi bo'lishi kerak (BR-USR-02).");
        }

        var tenant = new Tenant { Code = tenantCode, Name = tenantName };
        db.Tenants.Add(tenant);
        db.Users.Add(new User
        {
            TenantId = tenant.Id,
            Login = adminLogin.Trim().ToLowerInvariant(),
            FullName = adminFullName,
            Role = UserRole.Admin,
            PasswordHash = hasher.Hash(adminPassword),
            MustChangePassword = true,
        });
        await db.SaveChangesAsync(ct);
    }
}
