using Mawasem.Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure( EntityTypeBuilder<Review> builder )
    {
        builder.ToTable("Reviews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Rating)
            .IsRequired();

        builder.Property(x => x.Comment)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Multiple reviews from the same customer for the same product
        // are allowed. This index is intentionally not unique.
        builder.HasIndex(x => new
        {
            x.ProductId ,
            x.UserId
        });

        // Soft-deleted reviews are hidden from normal queries.
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}