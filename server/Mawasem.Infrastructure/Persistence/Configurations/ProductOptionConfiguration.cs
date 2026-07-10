using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class ProductOptionConfiguration : IEntityTypeConfiguration<ProductOption>
{
    public void Configure( EntityTypeBuilder<ProductOption> builder )
    {
        builder.ToTable("ProductOptions");

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
    }
}