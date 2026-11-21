using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SmartAccess.Application.Abstractions;
using SmartAccess.Domain.Settings;

namespace SmartAccess.Application.Settings;

/// <summary>Sozlamani o'qish: bazada bo'lmasa CLAUDE.md 4.17 standart qiymati.</summary>
public sealed class SettingsReader(IAppDbContext db)
{
    public async Task<string> GetAsync(Guid tenantId, string key, CancellationToken ct = default)
    {
        var value = await db.Settings.IgnoreQueryFilters()
            .Where(s => s.TenantId == tenantId && s.Key == key)
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct);
        return value ?? SettingKeys.Defaults[key];
    }

    public async Task<int> GetIntAsync(Guid tenantId, string key, CancellationToken ct = default) =>
        int.Parse(await GetAsync(tenantId, key, ct), CultureInfo.InvariantCulture);

    public async Task<bool> GetBoolAsync(Guid tenantId, string key, CancellationToken ct = default) =>
        bool.Parse(await GetAsync(tenantId, key, ct));
}
