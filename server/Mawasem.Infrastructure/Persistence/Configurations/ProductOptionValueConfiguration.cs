using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class ProductOptionValueConfiguration : IEntityTypeConfiguration<ProductOptionValue>
{
    public void Configure( EntityTypeBuilder<ProductOptionValue> builder )
    {
        builder.ToTable("ProductOptionValues");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Value , value =>
        {
            value.Property(x => x.English)
                 .HasColumnName("ValueEn")
                 .HasMaxLength(100)
                 .IsRequired();

            value.Property(x => x.Arabic)
                 .HasColumnName("ValueAr")
                 .HasMaxLength(100)
                 .IsRequired();
        });

        builder.HasOne(x => x.ProductOption)
               .WithMany(x => x.Values)
               .HasForeignKey(x => x.ProductOptionId);
    }
}