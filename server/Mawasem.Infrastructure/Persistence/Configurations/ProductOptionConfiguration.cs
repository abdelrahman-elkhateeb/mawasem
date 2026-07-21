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

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(x => x.Type)
            .IsUnique()
            .HasFilter("[Type] = 2")
            .HasDatabaseName("UX_ProductOptions_SingleColorOption");
    }
}