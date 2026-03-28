using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicePulseMonitor.Data.Models;

namespace ServicePulseMonitor.Data.Configurations;

/// <summary>EF Core entity configuration for the <see cref="ServiceDependency"/> model.</summary>
public class ServiceDependencyConfiguration : IEntityTypeConfiguration<ServiceDependency>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ServiceDependency> builder)
    {
        builder.ToTable("service_dependencies");

        builder.HasKey(sd => sd.DependencyId);
        builder.Property(sd => sd.DependencyId)
            .HasColumnName("dependency_id")
            .UseIdentityAlwaysColumn();

        builder.Property(sd => sd.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();

        builder.Property(sd => sd.DependsOnServiceId)
            .HasColumnName("depends_on_service_id")
            .IsRequired();

        builder.Property(sd => sd.DiscoveredAt)
            .HasColumnName("discovered_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasIndex(sd => new { sd.ServiceId, sd.DependsOnServiceId }).IsUnique();

        builder.HasOne(sd => sd.Caller)
            .WithMany(s => s.CalledServices)
            .HasForeignKey(sd => sd.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sd => sd.Callee)
            .WithMany(s => s.CallingServices)
            .HasForeignKey(sd => sd.DependsOnServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
