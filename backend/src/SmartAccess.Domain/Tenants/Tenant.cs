using SmartAccess.Domain.Common;

namespace SmartAccess.Domain.Tenants;

public sealed class Tenant : Entity
{
    /// <summary>Korxona identifikatori (login oynasidagi 1-maydon). Unikal.</summary>
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string Timezone { get; set; } = "Asia/Tashkent";
}
