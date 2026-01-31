using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicePulseMonitor.Models;

namespace ServicePulseMonitor.Data.Configurations;

public class ServiceDependencyConfiguration : IEntityTypeConfiguration<ServiceDependency>
{
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

        builder.HasIndex(sd => sd.ServiceId);
        builder.HasIndex(sd => sd.DependsOnServiceId);

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
