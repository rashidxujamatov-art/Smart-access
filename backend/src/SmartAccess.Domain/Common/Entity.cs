namespace SmartAccess.Domain.Common;

/// <summary>CLAUDE.md 6-bo'lim: barcha jadvallarda id (uuid), tenant_id, created_at, updated_at.</summary>
public abstract class Entity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>Muassasaga tegishli yozuv. Tenant filtri AppDbContext'da avtomatik qo'llanadi.</summary>
public abstract class TenantEntity : Entity
{
    public Guid TenantId { get; set; }
}

/// <summary>O'chiriladigan yozuv — soft delete (deleted_at).</summary>
public interface ISoftDeletable
{
    DateTimeOffset? DeletedAt { get; set; }
}
