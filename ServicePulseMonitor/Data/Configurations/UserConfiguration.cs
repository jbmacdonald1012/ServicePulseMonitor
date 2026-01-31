using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicePulseMonitor.Data.Models;

namespace ServicePulseMonitor.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.UserGuid);
        builder.Property(u => u.UserGuid)
            .HasColumnName("user_guid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(u => u.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(255);

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.AccessLevel)
            .HasColumnName("access_level")
            .HasMaxLength(50);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasIndex(u => u.Username).IsUnique();

        builder.HasMany(u => u.Alerts)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserGuid)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
