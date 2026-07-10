using Mawasem.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class RefundRequestItemConfiguration
    : IEntityTypeConfiguration<RefundRequestItem>
{
    public void Configure( EntityTypeBuilder<RefundRequestItem> builder )
    {
        builder.ToTable(
            "RefundRequestItems" ,
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_RefundRequestItems_Quantity_Positive" ,
                    "[Quantity] > 0");
            });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(1000);

        // Deleting a refund request deletes its request items.
        builder.HasOne(x => x.RefundRequest)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.RefundRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Historical order items must remain available.
        builder.HasOne(x => x.OrderItem)
            .WithMany()
            .HasForeignKey(x => x.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RefundRequestId);

        builder.HasIndex(x => x.OrderItemId);

        // The same order item cannot appear twice
        // in the same refund request.
        builder.HasIndex(x => new
        {
            x.RefundRequestId ,
            x.OrderItemId
        })
        .IsUnique();
    }
}