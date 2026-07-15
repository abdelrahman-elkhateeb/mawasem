using Mawasem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class UserPermissionConfiguration
    : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(
        EntityTypeBuilder<UserPermission> builder )
    {
        builder.ToTable("UserPermissions");

        builder.HasKey(x => new
        {
            x.UserId ,
            x.PermissionId
        });

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserPermissions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
            .WithMany(x => x.UserPermissions)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PermissionId);
    }
}