using Mawasem.API.Authorization;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace Mawasem.Tests.Authorization;

public sealed class DirectUserPermissionAuthorizationTests
{
    [Fact]
    public async Task HandleAsync_Succeeds_WhenPermissionIsAssignedDirectlyToEmployee()
    {
        var options =
            new DbContextOptionsBuilder<MawasemDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString("N"))
                .Options;

        await using var dbContext =
            new MawasemDbContext(options);

        var user =
            new ApplicationUser
            {
                Id = 10 ,
                UserName = "direct-permission-user" ,
                NormalizedUserName =
                    "DIRECT-PERMISSION-USER" ,
                FullNameAr = "مستخدم بصلاحية مباشرة" ,
                FullNameEn =
                    "Direct Permission User" ,
                IsBlocked = false
            };

        var role =
            new ApplicationRole
            {
                Id = 10 ,
                Name = SystemRoles.SalesEmployee ,
                NormalizedName =
                    SystemRoles.SalesEmployee
                        .ToUpperInvariant()
            };

        var permission =
            new Permission
            {
                Id = 10 ,
                Name =
                    SystemPermissions.Products.Delete ,
                Description =
                    "Direct permission test." ,
                CreatedOn =
                    DateTimeOffset.UtcNow
            };

        dbContext.Users.Add(user);
        dbContext.Roles.Add(role);
        dbContext.Permissions.Add(permission);

        dbContext.UserRoles.Add(
            new IdentityUserRole<int>
            {
                UserId = user.Id ,
                RoleId = role.Id
            });

        dbContext.UserPermissions.Add(
            new UserPermission
            {
                UserId = user.Id ,
                PermissionId = permission.Id
            });

        await dbContext.SaveChangesAsync();

        var requirement =
            new PermissionAuthorizationRequirement(
                SystemPermissions.Products.Delete);

        var identity =
            new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier ,
                        user.Id.ToString(
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

        Assert.True(
            authorizationContext.HasSucceeded);
    }
}