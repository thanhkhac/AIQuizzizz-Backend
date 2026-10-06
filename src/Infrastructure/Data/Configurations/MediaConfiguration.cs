using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitectureBase.Infrastructure.Data.Configurations;

public class MediaConfiguration : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.ModerationStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.ObjectKey).IsRequired().HasMaxLength(300);
        builder.Property(m => m.ThumbnailKey).HasMaxLength(300);
        builder.Property(m => m.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(m => m.OriginalFileName).HasMaxLength(500);
        builder.Property(m => m.ModerationResultJson).HasColumnType("json");

        // job kiểm duyệt quét theo trạng thái
        builder.HasIndex(m => new { m.Type, m.ModerationStatus });

        builder.HasOne(m => m.Owner)
            .WithMany()
            .HasForeignKey(m => m.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Questions)
            .WithOne(q => q.Media)
            .HasForeignKey(q => q.MediaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class UserViolationConfiguration : IEntityTypeConfiguration<UserViolation>
{
    public void Configure(EntityTypeBuilder<UserViolation> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Reason).IsRequired().HasMaxLength(500);
        builder.Property(v => v.DetailJson).HasColumnType("json");

        builder.HasIndex(v => new { v.UserId, v.IsExpired, v.ExpiresAt });

        builder.HasOne(v => v.User)
            .WithMany(u => u.Violations)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
