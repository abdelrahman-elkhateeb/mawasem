using Mawasem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations.Identity;

public sealed class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(
        EntityTypeBuilder<ApplicationUser> builder )
    {
        builder.Property(x => x.FullNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.FullNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.BirthDate);

        builder.Property(x => x.Gender)
            .HasConversion<int>();

        builder.Property(x => x.ReferralSource)
            .HasConversion<int>();

        builder.Property(x => x.IsBlocked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.BlockedAt);

        builder.Property(x => x.BlockedReason)
            .HasMaxLength(500);

        builder.Property(x => x.MustChangePassword)
            .IsRequired()
            .HasDefaultValue(false);

        // Phone numbers will be normalized before they are saved.
        // The unique filtered index prevents reuse while still allowing
        // admin accounts that do not have a phone number.

        builder.HasIndex(x => x.PhoneNumber)
            .IsUnique()
            .HasFilter("[PhoneNumber] IS NOT NULL");

        builder.HasIndex(x => x.IsBlocked);
    }
}