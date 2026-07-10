using Mawasem.Domain.Catalog;
using Mawasem.Domain.Delivery;
using Mawasem.Domain.Identity;
using Mawasem.Domain.Orders;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Persistence.Contexts;

public class MawasemDbContext
    : IdentityDbContext<ApplicationUser , ApplicationRole , int>
{
    public MawasemDbContext(
        DbContextOptions<MawasemDbContext> options )
        : base(options)
    {
    }

    #region Catalog

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductVariant> ProductVariants =>
        Set<ProductVariant>();

    public DbSet<ProductImage> ProductImages =>
        Set<ProductImage>();

    public DbSet<ProductSpecification> ProductSpecifications =>
        Set<ProductSpecification>();

    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Collection> Collections => Set<Collection>();

    public DbSet<Season> Seasons => Set<Season>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<Grade> Grades => Set<Grade>();

    public DbSet<ProductCategory> ProductCategories =>
        Set<ProductCategory>();

    public DbSet<ProductCollection> ProductCollections =>
        Set<ProductCollection>();

    public DbSet<ProductTag> ProductTags =>
        Set<ProductTag>();

    public DbSet<ProductGrade> ProductGrades =>
        Set<ProductGrade>();

    #endregion

    #region Delivery

    public DbSet<DeliveryArea> DeliveryAreas =>
        Set<DeliveryArea>();

    public DbSet<UserAddress> UserAddresses =>
        Set<UserAddress>();

    #endregion

    #region Orders

    public DbSet<Order> Orders =>
        Set<Order>();

    public DbSet<OrderItem> OrderItems =>
        Set<OrderItem>();

    public DbSet<RefundRequest> RefundRequests =>
        Set<RefundRequest>();

    public DbSet<RefundRequestItem> RefundRequestItems =>
        Set<RefundRequestItem>();

    #endregion

    #region Authorization

    public DbSet<Permission> Permissions =>
        Set<Permission>();

    public DbSet<RolePermission> RolePermissions =>
        Set<RolePermission>();

    #endregion

    protected override void OnModelCreating( ModelBuilder builder )
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(MawasemDbContext).Assembly);
    }
}