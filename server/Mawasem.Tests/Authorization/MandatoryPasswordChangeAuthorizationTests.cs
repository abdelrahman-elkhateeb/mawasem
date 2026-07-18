using Mawasem.API.Authorization;
using Mawasem.Application.Features.Authentication.Models;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace Mawasem.Tests.Authorization;

public sealed class MandatoryPasswordChangeAuthorizationTests
{
    [Fact]
    public async Task HandleAsync_DoesNotSucceed_WhenPasswordChangeIsRequired()
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
                Id = 20 ,
                UserName =
                    "temporary-password-user" ,
                NormalizedUserName =
                    "TEMPORARY-PASSWORD-USER" ,
                FullNameAr =
                    "مستخدم كلمة مرور مؤقتة" ,
                FullNameEn =
                    "Temporary Password User" ,
                IsBlocked = false ,
                MustChangePassword = true
            };

        var role =
            new ApplicationRole
            {
                Id = 20 ,
                Name =
                    SystemRoles.SalesEmployee ,
                NormalizedName =
                    SystemRoles.SalesEmployee
                        .ToUpperInvariant()
            };

        var permission =
            new Permission
            {
                Id = 20 ,
                Name =
                    SystemPermissions.Products.View ,
                Description =
                    "Mandatory password-change test." ,
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

        dbContext.RolePermissions.Add(
            new RolePermission
            {
                RoleId = role.Id ,
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
                        JwtClaimNames.Subject ,
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

        Assert.False(
            authorizationContext.HasSucceeded);
    }
}
