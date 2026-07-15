using Mawasem.Application.Features.Roles.Contracts.Requests;
using Mawasem.Application.Features.Roles.Models;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Mawasem.Infrastructure.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Tests.Roles;

public sealed class RolePermissionManagementServiceTests
{
    [Theory]
    [InlineData(SystemRoles.Customer)]
    [InlineData(SystemRoles.SuperAdmin)]
    public async Task UpdatePermissionsAsync_DoesNotModifyProtectedRole(
        string protectedRoleName )
    {
        await using var dbContext =
            CreateDbContext();

        var superAdminRole =
            CreateRole(
                id: 1 ,
                SystemRoles.SuperAdmin);

        var customerRole =
            CreateRole(
                id: 2 ,
                SystemRoles.Customer);

        var actor =
            CreateUser(
                id: 1 ,
                email: "superadmin@example.com");

        dbContext.Roles.AddRange(
            superAdminRole ,
            customerRole);

        dbContext.Users.Add(actor);

        dbContext.UserRoles.Add(
            new IdentityUserRole<int>
            {
                UserId = actor.Id ,
                RoleId = superAdminRole.Id
            });

        await dbContext.SaveChangesAsync();

        var service =
            new RolePermissionManagementService(
                dbContext);

        var result =
            await service.UpdatePermissionsAsync(
                actor.Id ,
                protectedRoleName ,
                new UpdateRolePermissionsRequest());

        Assert.False(result.Succeeded);

        Assert.Equal(
            RoleManagementErrorCodes.ProtectedRole ,
            result.ErrorCode);
    }

    [Fact]
    public async Task UpdatePermissionsAsync_AlwaysPreservesDashboardAccess()
    {
        await using var dbContext =
            CreateDbContext();

        var superAdminRole =
            CreateRole(
                id: 10 ,
                SystemRoles.SuperAdmin);

        var storeRole =
            CreateRole(
                id: 11 ,
                SystemRoles.StoreEmployee);

        var dashboardAccess =
            CreatePermission(
                id: 10 ,
                SystemPermissions.Dashboard.Access);

        var productsView =
            CreatePermission(
                id: 11 ,
                SystemPermissions.Products.View);

        var actor =
            CreateUser(
                id: 10 ,
                email: "superadmin@example.com");

        dbContext.Roles.AddRange(
            superAdminRole ,
            storeRole);

        dbContext.Permissions.AddRange(
            dashboardAccess ,
            productsView);

        dbContext.Users.Add(actor);

        dbContext.UserRoles.Add(
            new IdentityUserRole<int>
            {
                UserId = actor.Id ,
                RoleId = superAdminRole.Id
            });

        dbContext.RolePermissions.Add(
            new RolePermission
            {
                RoleId = storeRole.Id ,
                PermissionId =
                    dashboardAccess.Id
            });

        await dbContext.SaveChangesAsync();

        var service =
            new RolePermissionManagementService(
                dbContext);

        var addResult =
            await service.UpdatePermissionsAsync(
                actor.Id ,
                SystemRoles.StoreEmployee ,
                new UpdateRolePermissionsRequest
                {
                    PermissionNames =
                        new[]
                        {
                            SystemPermissions.Products.View
                        }
                });

        Assert.True(addResult.Succeeded);
        Assert.NotNull(addResult.Response);

        Assert.Contains(
            SystemPermissions.Dashboard.Access ,
            addResult.Response.PermissionNames);

        Assert.Contains(
            SystemPermissions.Products.View ,
            addResult.Response.PermissionNames);

        var clearResult =
            await service.UpdatePermissionsAsync(
                actor.Id ,
                SystemRoles.StoreEmployee ,
                new UpdateRolePermissionsRequest
                {
                    PermissionNames =
                        Array.Empty<string>()
                });

        Assert.True(clearResult.Succeeded);
        Assert.NotNull(clearResult.Response);

        Assert.Equal(
            new[]
            {
                SystemPermissions.Dashboard.Access
            } ,
            clearResult.Response.PermissionNames);
    }

    [Fact]
    public async Task UpdatePermissionsAsync_DoesNotAllowAdminToGrantPermissionTheyDoNotHold()
    {
        await using var dbContext =
            CreateDbContext();

        var adminRole =
            CreateRole(
                id: 20 ,
                SystemRoles.Admin);

        var storeRole =
            CreateRole(
                id: 21 ,
                SystemRoles.StoreEmployee);

        var dashboardAccess =
            CreatePermission(
                id: 20 ,
                SystemPermissions.Dashboard.Access);

        var manageRolePermissions =
            CreatePermission(
                id: 21 ,
                SystemPermissions.Roles.ManagePermissions);

        var ordersView =
            CreatePermission(
                id: 22 ,
                SystemPermissions.Orders.View);

        var actor =
            CreateUser(
                id: 20 ,
                email: "admin@example.com");

        dbContext.Roles.AddRange(
            adminRole ,
            storeRole);

        dbContext.Permissions.AddRange(
            dashboardAccess ,
            manageRolePermissions ,
            ordersView);

        dbContext.Users.Add(actor);

        dbContext.UserRoles.Add(
            new IdentityUserRole<int>
            {
                UserId = actor.Id ,
                RoleId = adminRole.Id
            });

        dbContext.RolePermissions.AddRange(
            new RolePermission
            {
                RoleId = adminRole.Id ,
                PermissionId =
                    dashboardAccess.Id
            } ,
            new RolePermission
            {
                RoleId = adminRole.Id ,
                PermissionId =
                    manageRolePermissions.Id
            } ,
            new RolePermission
            {
                RoleId = storeRole.Id ,
                PermissionId =
                    dashboardAccess.Id
            });

        await dbContext.SaveChangesAsync();

        var service =
            new RolePermissionManagementService(
                dbContext);

        var result =
            await service.UpdatePermissionsAsync(
                actor.Id ,
                SystemRoles.StoreEmployee ,
                new UpdateRolePermissionsRequest
                {
                    PermissionNames =
                        new[]
                        {
                            SystemPermissions.Orders.View
                        }
                });

        Assert.False(result.Succeeded);

        Assert.Equal(
            RoleManagementErrorCodes.InvalidPermission ,
            result.ErrorCode);

        var storePermissionIds =
            await dbContext.RolePermissions
                .Where(rolePermission =>
                    rolePermission.RoleId ==
                        storeRole.Id)
                .Select(rolePermission =>
                    rolePermission.PermissionId)
                .ToArrayAsync();

        Assert.Equal(
            new[]
            {
                dashboardAccess.Id
            } ,
            storePermissionIds);
    }

    private static MawasemDbContext
        CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<MawasemDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString("N"))
                .Options;

        return new MawasemDbContext(options);
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

    private static ApplicationUser CreateUser(
        int id ,
        string email )
    {
        return new ApplicationUser
        {
            Id = id ,
            UserName = email ,
            NormalizedUserName =
                email.ToUpperInvariant() ,
            Email = email ,
            NormalizedEmail =
                email.ToUpperInvariant() ,
            FullNameAr =
                "مستخدم لوحة التحكم" ,
            FullNameEn =
                "Dashboard User" ,
            IsBlocked = false ,
            MustChangePassword = false
        };
    }
}