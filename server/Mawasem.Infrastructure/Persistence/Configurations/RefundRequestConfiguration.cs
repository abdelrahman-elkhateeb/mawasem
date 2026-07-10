using Mawasem.Domain.Identity;
using Mawasem.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class RefundRequestConfiguration
    : IEntityTypeConfiguration<RefundRequest>
{
    public void Configure( EntityTypeBuilder<RefundRequest> builder )
    {
        builder.ToTable("RefundRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CustomerReason)
            .HasMaxLength(1000);

        builder.Property(x => x.AdminNotes)
            .HasMaxLength(2000);

        builder.Property(x => x.RequestedAt)
            .IsRequired();

        builder.Property(x => x.ReviewedAt);

        builder.Property(x => x.ReviewedByEmployeeId);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.RefundRequests)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.ReviewedByEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.OrderId);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.RequestedAt);

        builder.HasIndex(x => x.ReviewedByEmployeeId);

        builder.HasIndex(x => new
        {
            x.OrderId ,
            x.Status
        });
    }
}