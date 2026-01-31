using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicePulseMonitor.Data.Models;

namespace ServicePulseMonitor.Data.Configurations;

public class NotificationConfigConfiguration : IEntityTypeConfiguration<NotificationConfig>
{
    public void Configure(EntityTypeBuilder<NotificationConfig> builder)
    {
        builder.ToTable("notification_configs");

        builder.HasKey(nc => nc.NotificationConfigGuid);
        builder.Property(nc => nc.NotificationConfigGuid)
            .HasColumnName("notification_config_guid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(nc => nc.RuleId)
            .HasColumnName("rule_id")
            .IsRequired();

        builder.Property(nc => nc.Uri)
            .HasColumnName("uri")
            .HasMaxLength(500);

        builder.Property(nc => nc.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(255);

        builder.Property(nc => nc.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasIndex(nc => nc.RuleId);

        builder.HasOne(nc => nc.AlertRule)
            .WithMany(ar => ar.NotificationConfigs)
            .HasForeignKey(nc => nc.RuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
