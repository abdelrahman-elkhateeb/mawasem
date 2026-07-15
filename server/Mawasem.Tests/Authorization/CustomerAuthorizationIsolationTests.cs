using Mawasem.API.Authorization;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace Mawasem.Tests.Authorization;

public sealed class CustomerAuthorizationIsolationTests
{
    [Fact]
    public async Task HandleAsync_DoesNotSucceed_WhenCustomerHasDirectPermission()
    {
        var options =
            new DbContextOptionsBuilder<MawasemDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString("N"))
                .Options;

        await using var dbContext =
            new MawasemDbContext(options);

        var customer =
            new ApplicationUser
            {
                Id = 30 ,
                UserName = "customer" ,
                NormalizedUserName = "CUSTOMER" ,
                FullNameAr = "عميل" ,
                FullNameEn = "Customer" ,
                IsBlocked = false ,
                MustChangePassword = false
            };

        var customerRole =
            new ApplicationRole
            {
                Id = 30 ,
                Name = SystemRoles.Customer ,
                NormalizedName =
                    SystemRoles.Customer
                        .ToUpperInvariant()
            };

        var permission =
            new Permission
            {
                Id = 30 ,
                Name =
                    SystemPermissions.Products.View ,
                Description =
                    "Customer isolation test." ,
                CreatedOn =
                    DateTimeOffset.UtcNow
            };

        dbContext.Users.Add(customer);
        dbContext.Roles.Add(customerRole);
        dbContext.Permissions.Add(permission);

        dbContext.UserRoles.Add(
            new IdentityUserRole<int>
            {
                UserId = customer.Id ,
                RoleId = customerRole.Id
            });

        // Even an accidental direct assignment must remain ineffective
        // because this account has no dashboard role.
        dbContext.UserPermissions.Add(
            new UserPermission
            {
                UserId = customer.Id ,
                PermissionId = permission.Id
            });

        await dbContext.SaveChangesAsync();

        var requirement =
            new PermissionAuthorizationRequirement(
                SystemPermissions.Products.View);

        var identity =
            new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier ,
                        customer.Id.ToString(
                            CultureInfo.InvariantCulture))
                } ,
                authenticationType:
                    "TestAuthentication");

        var authorizationContext =
            new AuthorizationHandlerContext(
                new[] { requirement } ,
                new ClaimsPrincipal(identity) ,
                resource: null);

        var handler =
            new PermissionAuthorizationHandler(
                dbContext);

        await handler.HandleAsync(
            authorizationContext);

        Assert.False(
            authorizationContext.HasSucceeded);
    }
}