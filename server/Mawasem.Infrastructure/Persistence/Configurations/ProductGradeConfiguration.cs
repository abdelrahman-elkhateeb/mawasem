using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class ProductGradeConfiguration
    : IEntityTypeConfiguration<ProductGrade>
{
    public void Configure( EntityTypeBuilder<ProductGrade> builder )
    {
        builder.ToTable("ProductGrades");

        builder.HasKey(x => new
        {
            x.ProductId ,
            x.GradeId
        });

        builder.HasOne(x => x.Product)
            .WithMany(x => x.ProductGrades)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Grade)
            .WithMany(x => x.ProductGrades)
            .HasForeignKey(x => x.GradeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.GradeId);
    }
}