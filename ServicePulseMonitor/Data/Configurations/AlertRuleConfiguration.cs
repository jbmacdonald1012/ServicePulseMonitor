using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicePulseMonitor.Models;

namespace ServicePulseMonitor.Data.Configurations;

public class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
{
    public void Configure(EntityTypeBuilder<AlertRule> builder)
    {
        builder.ToTable("alert_rules");

        builder.HasKey(ar => ar.RuleId);
        builder.Property(ar => ar.RuleId)
            .HasColumnName("rule_id")
            .UseIdentityAlwaysColumn();

        builder.Property(ar => ar.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();

        builder.Property(ar => ar.RuleType)
            .HasColumnName("rule_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ar => ar.Threshold)
            .HasColumnName("threshold")
            .HasColumnType("decimal(10,2)");

        builder.Property(ar => ar.LogType)
            .HasColumnName("log_type")
            .HasMaxLength(50);

        builder.Property(ar => ar.NotificationChannel)
            .HasColumnName("notification_channel")
            .HasMaxLength(100);

        builder.HasIndex(ar => ar.ServiceId);

        builder.HasOne(ar => ar.Service)
            .WithMany(s => s.AlertRules)
            .HasForeignKey(ar => ar.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ar => ar.NotificationConfigs)
            .WithOne(nc => nc.AlertRule)
            .HasForeignKey(nc => nc.RuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
