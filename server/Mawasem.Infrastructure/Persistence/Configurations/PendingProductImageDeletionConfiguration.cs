using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class PendingProductImageDeletionConfiguration :
    IEntityTypeConfiguration<PendingProductImageDeletion>
{
    public void Configure(
        EntityTypeBuilder<PendingProductImageDeletion> builder )
    {
        builder.ToTable(
            "PendingProductImageDeletions" ,
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_PendingProductImageDeletions_AttemptCount" ,
                    "[AttemptCount] >= 0");
            });

        builder.HasKey(
            deletion => deletion.Id);

        builder.Property(
                deletion => deletion.StorageKey)
            .HasMaxLength(
                PendingProductImageDeletion.MaxStorageKeyLength)
            .IsRequired();

        builder.Property(
                deletion => deletion.CreatedAt)
            .IsRequired();

        builder.Property(
                deletion => deletion.AttemptCount)
            .IsRequired();

        builder.Property(
                deletion => deletion.NextAttemptAt)
            .IsRequired();

        builder.Property(
                deletion => deletion.LastError)
            .HasMaxLength(
                PendingProductImageDeletion.MaxLastErrorLength);

        builder.HasIndex(
                deletion => deletion.StorageKey)
            .IsUnique();

        builder.HasIndex(
            deletion =>
                new
                {
                    deletion.NextAttemptAt ,
                    deletion.Id
                });
    }
}