using Mawasem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations.Identity;

public sealed class PasswordResetCodeConfiguration
    : IEntityTypeConfiguration<PasswordResetCode>
{
    public void Configure(
        EntityTypeBuilder<PasswordResetCode> builder )
    {
        builder.ToTable("PasswordResetCodes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Channel)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CodeHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.ResetTokenHash)
            .HasMaxLength(128);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.FailedAttempts)
            .IsRequired();

        builder.Property(x => x.RequestedByIp)
            .HasMaxLength(45);

        builder.HasIndex(x => new
        {
            x.UserId ,
            x.Channel ,
            x.ExpiresAtUtc
        });

        builder.HasIndex(x => x.ExpiresAtUtc);

        builder.HasIndex(x => x.ResetTokenHash)
            .IsUnique()
            .HasFilter("[ResetTokenHash] IS NOT NULL");

        builder.HasOne(x => x.User)
            .WithMany(x => x.PasswordResetCodes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.IsExpired);
        builder.Ignore(x => x.IsActive);
        builder.Ignore(x => x.IsResetTokenActive);
    }
}