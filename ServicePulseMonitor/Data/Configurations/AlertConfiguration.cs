using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicePulseMonitor.Data.Models;

namespace ServicePulseMonitor.Data.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("alerts");

        builder.HasKey(a => a.AlertId);
        builder.Property(a => a.AlertId)
            .HasColumnName("alert_id")
            .UseIdentityAlwaysColumn();

        builder.Property(a => a.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();

        builder.Property(a => a.UserGuid)
            .HasColumnName("user_guid")
            .IsRequired();

        builder.Property(a => a.AlertType)
            .HasColumnName("alert_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.TriggeredAt)
            .HasColumnName("triggered_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(a => a.IsAcknowledged)
            .HasColumnName("is_acknowledged")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.IsResolved)
            .HasColumnName("is_resolved")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.ResolvedAt)
            .HasColumnName("resolved_at")
            .HasColumnType("timestamptz");

        builder.Property(a => a.Message)
            .HasColumnName("message")
            .HasColumnType("jsonb");

        builder.HasIndex(a => a.ServiceId);
        builder.HasIndex(a => a.TriggeredAt);
        builder.HasIndex(a => a.IsResolved);

        builder.HasOne(a => a.Service)
            .WithMany(s => s.Alerts)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.User)
            .WithMany(u => u.Alerts)
            .HasForeignKey(a => a.UserGuid)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
