using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicePulseMonitor.Data.Models;

namespace ServicePulseMonitor.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(s => s.ServiceId);
        builder.Property(s => s.ServiceId)
            .HasColumnName("service_id")
            .UseIdentityAlwaysColumn();

        builder.Property(s => s.ServiceName)
            .HasColumnName("service_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(s => s.BaseUrl)
            .HasColumnName("base_url")
            .HasMaxLength(500);

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(s => s.RegisteredAt)
            .HasColumnName("registered_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(s => s.LastSeenAt)
            .HasColumnName("last_seen_at")
            .HasColumnType("timestamptz");

        builder.HasIndex(s => s.ServiceName).IsUnique();
        builder.HasIndex(s => s.LastSeenAt);

        builder.HasMany(s => s.HealthChecks)
            .WithOne(h => h.Service)
            .HasForeignKey(h => h.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Alerts)
            .WithOne(a => a.Service)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.AlertRules)
            .WithOne(ar => ar.Service)
            .HasForeignKey(ar => ar.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
