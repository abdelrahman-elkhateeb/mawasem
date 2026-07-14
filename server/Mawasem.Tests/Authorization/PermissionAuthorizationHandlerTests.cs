using Mawasem.API.Authorization;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace Mawasem.Tests.Authorization;

public sealed class PermissionAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleAsync_Succeeds_WhenSuperAdminHasRequiredPermission()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser(
                id: 1 ,
                isBlocked: false);

        var role =
            CreateRole(
                id: 1 ,
                name: SystemRoles.SuperAdmin);

        var permission =
            CreatePermission(
                id: 1 ,
                name: SystemPermissions.Products.Delete);

        AddUserWithRole(
            dbContext ,
            user ,
            role);

        dbContext.Permissions.Add(permission);

        dbContext.RolePermissions.Add(
            new RolePermission
            {
                RoleId = role.Id ,
                PermissionId = permission.Id
            });

        await dbContext.SaveChangesAsync();

        var requirement =
            new PermissionAuthorizationRequirement(
                SystemPermissions.Products.Delete);

        var authorizationContext =
            CreateAuthorizationContext(
                user.Id ,
                requirement);

        var handler =
            new PermissionAuthorizationHandler(
                dbContext);

        await handler.HandleAsync(
            authorizationContext);

        Assert.True(
            authorizationContext.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_DoesNotSucceed_WhenDashboardRoleLacksRequiredPermission()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser(
                id: 2 ,
                isBlocked: false);

        var role =
            CreateRole(
                id: 2 ,
                name: SystemRoles.SalesEmployee);

        var dashboardAccessPermission =
            CreatePermission(
                id: 2 ,
                name: SystemPermissions.Dashboard.Access);

        var productDeletePermission =
            CreatePermission(
                id: 3 ,
                name: SystemPermissions.Products.Delete);

        AddUserWithRole(
            dbContext ,
            user ,
            role);

        dbContext.Permissions.AddRange(
            dashboardAccessPermission ,
            productDeletePermission);

        dbContext.RolePermissions.Add(
            new RolePermission
            {
                RoleId = role.Id ,
                PermissionId =
                    dashboardAccessPermission.Id
            });

        await dbContext.SaveChangesAsync();

        var requirement =
            new PermissionAuthorizationRequirement(
                SystemPermissions.Products.Delete);

        var authorizationContext =
            CreateAuthorizationContext(
                user.Id ,
                requirement);

        var handler =
            new PermissionAuthorizationHandler(
                dbContext);

        await handler.HandleAsync(
            authorizationContext);

        Assert.False(
            authorizationContext.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_DoesNotSucceed_WhenUserIsBlocked()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser(
                id: 3 ,
                isBlocked: true);

        var role =
            CreateRole(
                id: 3 ,
                name: SystemRoles.SuperAdmin);

        var permission =
            CreatePermission(
                id: 4 ,
                name: SystemPermissions.Products.Delete);

        AddUserWithRole(
            dbContext ,
            user ,
            role);

        dbContext.Permissions.Add(permission);

        dbContext.RolePermissions.Add(
            new RolePermission
            {
                RoleId = role.Id ,
                PermissionId = permission.Id
            });

        await dbContext.SaveChangesAsync();

        var requirement =
            new PermissionAuthorizationRequirement(
                SystemPermissions.Products.Delete);

        var authorizationContext =
            CreateAuthorizationContext(
                user.Id ,
                requirement);

        var handler =
            new PermissionAuthorizationHandler(
                dbContext);

        await handler.HandleAsync(
            authorizationContext);

        Assert.False(
            authorizationContext.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_DoesNotSucceed_WhenPermissionIsSoftDeleted()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser(
                id: 4 ,
                isBlocked: false);

        var role =
            CreateRole(
                id: 4 ,
                name: SystemRoles.SuperAdmin);

        var permission =
            CreatePermission(
                id: 5 ,
                name: SystemPermissions.Products.Delete);

        permission.IsDeleted = true;
        permission.DeletedOn =
            DateTimeOffset.UtcNow;

        AddUserWithRole(
            dbContext ,
            user ,
            role);

        dbContext.Permissions.Add(permission);

        dbContext.RolePermissions.Add(
            new RolePermission
            {
                RoleId = role.Id ,
                PermissionId = permission.Id
            });

        await dbContext.SaveChangesAsync();

        var requirement =
            new PermissionAuthorizationRequirement(
                SystemPermissions.Products.Delete);

        var authorizationContext =
            CreateAuthorizationContext(
                user.Id ,
                requirement);

        var handler =
            new PermissionAuthorizationHandler(
                dbContext);

        await handler.HandleAsync(
            authorizationContext);

        Assert.False(
            authorizationContext.HasSucceeded);
    }

    private static MawasemDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<MawasemDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString("N"))
                .Options;

        return new MawasemDbContext(options);
    }

    private static ApplicationUser CreateUser(
        int id ,
        bool isBlocked )
    {
        return new ApplicationUser
        {
            Id = id ,
            UserName = $"user-{id}" ,
            NormalizedUserName =
                $"USER-{id}" ,
            FullNameAr = $"مستخدم {id}" ,
            FullNameEn = $"User {id}" ,
            IsBlocked = isBlocked
        };
    }

    private static ApplicationRole CreateRole(
        int id ,
        string name )
    {
        return new ApplicationRole
        {
            Id = id ,
            Name = name ,
            NormalizedName =
                name.ToUpperInvariant()
        };
    }

    private static Permission CreatePermission(
        int id ,
        string name )
    {
        return new Permission
        {
            Id = id ,
            Name = name ,
            Description =
                $"Test permission for {name}." ,
            CreatedOn =
                DateTimeOffset.UtcNow
        };
    }

    private static void AddUserWithRole(
        MawasemDbContext dbContext ,
        ApplicationUser user ,
        ApplicationRole role )
    {
        dbContext.Users.Add(user);
        dbContext.Roles.Add(role);

        dbContext.UserRoles.Add(
            new IdentityUserRole<int>
            {
                UserId = user.Id ,
                RoleId = role.Id
            });
    }

    private static AuthorizationHandlerContext
        CreateAuthorizationContext(
            int userId ,
            IAuthorizationRequirement requirement )
    {
        var claims =
            new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier ,
                    userId.ToString(
                        CultureInfo.InvariantCulture))
            };

        var identity =
            new ClaimsIdentity(
                claims ,
                authenticationType:
                    "TestAuthentication");

        var principal =
            new ClaimsPrincipal(identity);

        return new AuthorizationHandlerContext(
            new[] { requirement } ,
            principal ,
            resource: null);
    }
}