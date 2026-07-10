using Mawasem.Domain.Delivery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class DeliveryAreaConfiguration
    : IEntityTypeConfiguration<DeliveryArea>
{
    public void Configure( EntityTypeBuilder<DeliveryArea> builder )
    {
        builder.ToTable("DeliveryAreas");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Name , nameBuilder =>
        {
            nameBuilder.Property(x => x.English)
                .HasColumnName("NameEnglish")
                .HasMaxLength(200)
                .IsRequired();

            nameBuilder.Property(x => x.Arabic)
                .HasColumnName("NameArabic")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.Navigation(x => x.Name)
            .IsRequired();

        builder.Property(x => x.DeliveryFee)
            .HasPrecision(18 , 2)
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(x => x.IsFreeDelivery)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(x => x.IsActive);
    }
}