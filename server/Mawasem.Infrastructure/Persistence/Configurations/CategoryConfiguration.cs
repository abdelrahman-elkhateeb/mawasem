using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure( EntityTypeBuilder<Category> builder )
    {
        builder.ToTable("Categories");

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