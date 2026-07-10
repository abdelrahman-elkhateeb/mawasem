using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure( EntityTypeBuilder<Grade> builder )
    {
        builder.ToTable("Grades");

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