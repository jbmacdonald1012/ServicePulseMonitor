using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicePulseMonitor.Models;

namespace ServicePulseMonitor.Data.Configurations;

public class HealthCheckConfiguration : IEntityTypeConfiguration<HealthCheck>
{
    public void Configure(EntityTypeBuilder<HealthCheck> builder)
    {
        builder.ToTable("health_checks");

        builder.HasKey(h => h.HealthCheckId);
        builder.Property(h => h.HealthCheckId)
            .HasColumnName("health_check_id")
            .UseIdentityAlwaysColumn();

        builder.Property(h => h.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();

        builder.Property(h => h.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(h => h.ResponseTimeMs)
            .HasColumnName("response_time_ms");

        builder.Property(h => h.CheckedAt)
            .HasColumnName("checked_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(h => h.Details)
            .HasColumnName("details")
            .HasColumnType("jsonb");

        builder.HasCheckConstraint("CK_HealthCheck_Status",
            "status IN ('Healthy', 'Degraded', 'Unhealthy')");

        builder.HasIndex(h => h.ServiceId);
        builder.HasIndex(h => h.CheckedAt);
        builder.HasIndex(h => h.Status);

        builder.HasOne(h => h.Service)
            .WithMany(s => s.HealthChecks)
            .HasForeignKey(h => h.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
