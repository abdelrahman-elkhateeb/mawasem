using Mawasem.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure( EntityTypeBuilder<Order> builder )
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.OrderNumber)
            .IsUnique();

        builder.Property(x => x.CustomerNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CustomerNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CustomerPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.OrderDate)
            .IsRequired();

        builder.Property(x => x.SubTotal)
            .HasPrecision(18 , 2)
            .IsRequired();

        builder.Property(x => x.Discount)
            .HasPrecision(18 , 2)
            .IsRequired();

        builder.Property(x => x.DeliveryFee)
            .HasPrecision(18 , 2)
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18 , 2)
            .IsRequired();

        builder.Property(x => x.CouponCode)
            .HasMaxLength(100);

        builder.Property(x => x.OrderStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.PaymentStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.DeliveryMethod)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.OrderSource)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(500);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UserAddress)
            .WithMany()
            .HasForeignKey(x => x.UserAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.UserAddressId);

        builder.HasIndex(x => x.OrderDate);

        builder.HasIndex(x => x.OrderStatus);

        builder.HasIndex(x => x.PaymentStatus);

        builder.HasIndex(x => new
        {
            x.UserId ,
            x.OrderDate
        });
    }
}