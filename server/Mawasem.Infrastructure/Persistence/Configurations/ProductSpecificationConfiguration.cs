using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecification>
{
    public void Configure( EntityTypeBuilder<ProductSpecification> builder )
    {
        builder.ToTable("ProductSpecifications");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Name , name =>
        {
            name.Property(x => x.English)
                .HasColumnName("NameEn")
                .HasMaxLength(100)
                .IsRequired();

            name.Property(x => x.Arabic)
                .HasColumnName("NameAr")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(x => x.Value , value =>
        {
            value.Property(x => x.English)
                .HasColumnName("ValueEn")
                .HasMaxLength(500)
                .IsRequired();

            value.Property(x => x.Arabic)
                .HasColumnName("ValueAr")
                .HasMaxLength(500)
                .IsRequired();
        });

        builder.HasOne(x => x.Product)
               .WithMany(x => x.Specifications)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}