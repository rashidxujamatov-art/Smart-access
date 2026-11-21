using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmartAccess.Application.Abstractions;
using SmartAccess.Domain.Audit;
using SmartAccess.Domain.Common;
using SmartAccess.Domain.Settings;
using SmartAccess.Domain.Tenants;
using SmartAccess.Domain.Users;

namespace SmartAccess.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenant, TimeProvider time)
    : DbContext(options), IAppDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Setting> Settings => Set<Setting>();

    /// <summary>Filtr ifodasida ishlatiladi — har so'rovda joriy qiymat olinadi.</summary>
    private Guid? CurrentTenantId => tenant.TenantId;

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Tenant filtri (va soft delete) — barcha TenantEntity'lar uchun avtomatik.
        foreach (var et in b.Model.GetEntityTypes().Where(t => typeof(TenantEntity).IsAssignableFrom(t.ClrType)))
        {
            var method = typeof(AppDbContext)
                .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .MakeGenericMethod(et.ClrType);
            method.Invoke(this, [b]);
        }

        b.Entity<AuditLog>().HasQueryFilter(a => a.TenantId == CurrentTenantId);
    }

    private void ApplyTenantFilter<T>(ModelBuilder b)
        where T : TenantEntity
    {
        Expression<Func<T, bool>> filter = typeof(ISoftDeletable).IsAssignableFrom(typeof(T))
            ? e => e.TenantId == CurrentTenantId && ((ISoftDeletable)e).DeletedAt == null
            : e => e.TenantId == CurrentTenantId;
        b.Entity<T>().HasQueryFilter(filter);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = time.GetUtcNow();
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
