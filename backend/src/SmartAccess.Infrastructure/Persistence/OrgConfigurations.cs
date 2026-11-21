using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAccess.Application.Features.Org;
using SmartAccess.Domain.Org;
using SmartAccess.Domain.Tenants;

namespace SmartAccess.Infrastructure.Persistence;

/// <summary>
/// BR-ORG-05: nom unikalligi katta-kichik harfsiz — lower(name) bo'yicha unikal indekslar migratsiyada (SQL).
/// </summary>
internal sealed class DepartmentConfig : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.ToTable("departments");
        b.Property(x => x.Name).HasMaxLength(OrgService.NameMaxLength);
        b.HasIndex(x => x.TenantId);
        b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class GroupConfig : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> b)
    {
        b.ToTable("groups");
        b.Property(x => x.Name).HasMaxLength(OrgService.NameMaxLength);
        b.HasIndex(x => new { x.TenantId, x.DepartmentId });
        b.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
