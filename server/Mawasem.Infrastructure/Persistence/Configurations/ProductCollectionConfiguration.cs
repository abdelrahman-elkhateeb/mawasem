using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class ProductCollectionConfiguration
    : IEntityTypeConfiguration<ProductCollection>
{
    public void Configure( EntityTypeBuilder<ProductCollection> builder )
    {
        builder.ToTable("ProductCollections");

        builder.HasKey(x => new
        {
            x.ProductId ,
            x.CollectionId
        });

        builder.HasOne(x => x.Product)
            .WithMany(x => x.ProductCollections)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Collection)
            .WithMany(x => x.ProductCollections)
            .HasForeignKey(x => x.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CollectionId);
    }
}