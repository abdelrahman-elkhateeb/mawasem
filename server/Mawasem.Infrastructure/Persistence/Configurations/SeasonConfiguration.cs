using Mawasem.Modules.Season.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class SeasonConfiguration : IEntityTypeConfiguration<SeasonEntity>
{
    public void Configure( EntityTypeBuilder<SeasonEntity> builder )
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Type)
            .IsRequired();

        builder.Property(s => s.StartDate)
            .IsRequired();

        builder.Property(s => s.EndDate)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.BannerImageUrl)
            .HasMaxLength(2048);

        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.ToTable("Seasons");
    }
}