using Mawasem.Domain.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable(
            "Carts",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Carts_Owner",
                    """
                    (
                        [UserId] IS NOT NULL
                        AND [GuestTokenHash] IS NULL
                        AND [GuestExpiresOn] IS NULL
                    )
                    OR
                    (
                        [UserId] IS NULL
                        AND [GuestTokenHash] IS NOT NULL
                        AND [GuestExpiresOn] IS NOT NULL
                    )
                    """);
            });

        builder.HasKey(cart => cart.Id);

        builder.Property(cart => cart.GuestTokenHash)
            .HasColumnType("char(64)");

        builder.HasIndex(cart => cart.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");

        builder.HasIndex(cart => cart.GuestTokenHash)
            .IsUnique()
            .HasFilter("[GuestTokenHash] IS NOT NULL");

        builder.HasIndex(cart => cart.GuestExpiresOn);

        builder.HasOne(cart => cart.User)
            .WithOne()
            .HasForeignKey<Cart>(cart => cart.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(cart => cart.Items)
            .WithOne(item => item.Cart)
            .HasForeignKey(item => item.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
