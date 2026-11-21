using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAccess.Domain.Audit;
using SmartAccess.Domain.Settings;
using SmartAccess.Domain.Tenants;
using SmartAccess.Domain.Users;

namespace SmartAccess.Infrastructure.Persistence;

internal static class EnumText
{
    /// <summary>Enumlar bazada kichik harfli matn: guard/admin/manager, active/blocked (CLAUDE.md 6-bo'lim).</summary>
    public static PropertyBuilder<T> AsLowerText<T>(this PropertyBuilder<T> p)
        where T : struct, Enum =>
        p.HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<T>(v, true)).HasMaxLength(32);
}

internal sealed class TenantConfig : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("tenants");
        b.Property(x => x.Code).HasMaxLength(64);
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Name).HasMaxLength(200);
        b.Property(x => x.Timezone).HasMaxLength(64);
    }
}

internal sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.Property(x => x.Login).HasMaxLength(64);
        b.Property(x => x.FullName).HasMaxLength(200);
        b.Property(x => x.Role).AsLowerText();
        b.Property(x => x.Status).AsLowerText();
        b.Property(x => x.PasswordHash).HasMaxLength(256);
        b.Ignore(x => x.TwoFactorEnabled);

        // Login tenant ichida unikal (o'chirilganlar bundan mustasno — login qayta ishlatilishi mumkin).
        b.HasIndex(x => new { x.TenantId, x.Login }).IsUnique().HasFilter("deleted_at IS NULL");
        b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SessionConfig : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> b)
    {
        b.ToTable("sessions");
        b.Property(x => x.RefreshTokenHash).HasMaxLength(128);
        b.HasIndex(x => x.RefreshTokenHash).IsUnique();
        b.Property(x => x.Device).HasMaxLength(256);
        b.Property(x => x.Ip).HasMaxLength(64);
        b.HasIndex(x => new { x.TenantId, x.UserId });
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("audit_logs");
        b.Property(x => x.UserLogin).HasMaxLength(64);
        b.Property(x => x.UserFullName).HasMaxLength(200);
        b.Property(x => x.Role).HasMaxLength(32);
        b.Property(x => x.Ip).HasMaxLength(64);
        b.Property(x => x.Device).HasMaxLength(256);
        b.Property(x => x.Category).HasMaxLength(32);
        b.Property(x => x.Action).HasMaxLength(64);
        b.Property(x => x.ObjectType).HasMaxLength(64);
        b.Property(x => x.ObjectId).HasMaxLength(64);
        b.Property(x => x.OldValue).HasColumnType("jsonb");
        b.Property(x => x.NewValue).HasColumnType("jsonb");
        b.Property(x => x.Result).HasMaxLength(16);
        b.HasIndex(x => new { x.TenantId, x.Ts });
        b.HasIndex(x => new { x.TenantId, x.Category, x.Ts });
    }
}

internal sealed class SettingConfig : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> b)
    {
        b.ToTable("settings");
        b.Property(x => x.Key).HasMaxLength(128);
        b.Property(x => x.Value).HasColumnType("jsonb");
        b.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
    }
}
