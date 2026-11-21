using SmartAccess.Domain.Common;

namespace SmartAccess.Domain.Org;

/// <summary>BR-ORG-01: Bo'lim → Guruh → Xodim.</summary>
public sealed class Department : TenantEntity, ISoftDeletable
{
    public required string Name { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public sealed class Group : TenantEntity, ISoftDeletable
{
    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
