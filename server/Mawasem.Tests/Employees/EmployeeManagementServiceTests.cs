using Mawasem.Application.Features.Employees.Contracts.Requests;
using Mawasem.Application.Features.Employees.Models;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Employees;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Mawasem.Tests.Employees;

public sealed class EmployeeManagementServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesEmployeeWithRolesAndDirectPermissions()
    {
        await using var provider =
            CreateServiceProvider();

        await using var scope =
            provider.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    EmployeeManagementService>();

        var superAdminRole =
            CreateRole(
                id: 1 ,
                SystemRoles.SuperAdmin);

        var salesRole =
            CreateRole(
                id: 2 ,
                SystemRoles.SalesEmployee);

        var dashboardAccess =
            CreatePermission(
                id: 1 ,
                SystemPermissions.Dashboard.Access);

        var ordersView =
            CreatePermission(
                id: 2 ,
                SystemPermissions.Orders.View);

        dbContext.Roles.AddRange(
            superAdminRole ,
            salesRole);

        dbContext.Permissions.AddRange(
            dashboardAccess ,
            ordersView);

        dbContext.RolePermissions.Add(
            new RolePermission
            {
                RoleId = salesRole.Id ,
                PermissionId =
                    dashboardAccess.Id
            });

        await dbContext.SaveChangesAsync();

        var superAdmin =
            CreateDashboardUser(
                "superadmin@example.com");

        var createSuperAdminResult =
            await userManager.CreateAsync(
                superAdmin ,
                "SuperAdmin1!");

        Assert.True(
            createSuperAdminResult.Succeeded);

        var addSuperAdminRoleResult =
            await userManager.AddToRoleAsync(
                superAdmin ,
                SystemRoles.SuperAdmin);

        Assert.True(
            addSuperAdminRoleResult.Succeeded);

        var request =
            new CreateEmployeeRequest
            {
                FullNameAr =
                    "موظف مبيعات" ,
                FullNameEn =
                    "Sales Employee" ,
                Email =
                    "sales.employee@example.com" ,
                TemporaryPassword =
                    "Temporary1!" ,
                ConfirmTemporaryPassword =
                    "Temporary1!" ,
                RoleNames =
                    new[]
                    {
                        SystemRoles.SalesEmployee
                    } ,
                PermissionNames =
                    new[]
                    {
                        SystemPermissions.Orders.View
                    }
            };

        var result =
            await service.CreateAsync(
                superAdmin.Id ,
                request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Response);

        Assert.True(
            result.Response.MustChangePassword);

        Assert.Contains(
            SystemRoles.SalesEmployee ,
            result.Response.Roles);

        Assert.Contains(
            SystemPermissions.Orders.View ,
            result.Response.DirectPermissions);

        Assert.Contains(
            SystemPermissions.Dashboard.Access ,
            result.Response.EffectivePermissions);

        Assert.Contains(
            SystemPermissions.Orders.View ,
            result.Response.EffectivePermissions);

        var createdEmployee =
            await userManager.FindByEmailAsync(
                request.Email);

        Assert.NotNull(createdEmployee);

        Assert.True(
            await userManager.CheckPasswordAsync(
                createdEmployee ,
                request.TemporaryPassword));
    }

    [Fact]
    public async Task CreateAsync_DoesNotAllowAdminToGrantPermissionTheyDoNotHold()
    {
        await using var provider =
            CreateServiceProvider();

        await using var scope =
            provider.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    EmployeeManagementService>();

        var adminRole =
            CreateRole(
                id: 10 ,
                SystemRoles.Admin);

        var salesRole =
            CreateRole(
                id: 11 ,
                SystemRoles.SalesEmployee);

        var dashboardAccess =
            CreatePermission(
                id: 10 ,
                SystemPermissions.Dashboard.Access);

        var employeesCreate =
            CreatePermission(
                id: 11 ,
                SystemPermissions.Employees.Create);

        var ordersView =
            CreatePermission(
                id: 12 ,
                SystemPermissions.Orders.View);

        dbContext.Roles.AddRange(
            adminRole ,
            salesRole);

        dbContext.Permissions.AddRange(
            dashboardAccess ,
            employeesCreate ,
            ordersView);

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
                    employeesCreate.Id
            } ,
            new RolePermission
            {
                RoleId = salesRole.Id ,
                PermissionId =
                    dashboardAccess.Id
            });

        await dbContext.SaveChangesAsync();

        var admin =
            CreateDashboardUser(
                "admin@example.com");

        var createAdminResult =
            await userManager.CreateAsync(
                admin ,
                "AdminPass1!");

        Assert.True(
            createAdminResult.Succeeded);

        var addAdminRoleResult =
            await userManager.AddToRoleAsync(
                admin ,
                SystemRoles.Admin);

        Assert.True(
            addAdminRoleResult.Succeeded);

        var request =
            new CreateEmployeeRequest
            {
                FullNameAr =
                    "موظف غير مصرح" ,
                FullNameEn =
                    "Unauthorized Employee" ,
                Email =
                    "unauthorized.employee@example.com" ,
                TemporaryPassword =
                    "Temporary1!" ,
                ConfirmTemporaryPassword =
                    "Temporary1!" ,
                RoleNames =
                    new[]
                    {
                        SystemRoles.SalesEmployee
                    } ,
                PermissionNames =
                    new[]
                    {
                        SystemPermissions.Orders.View
                    }
            };

        var result =
            await service.CreateAsync(
                admin.Id ,
                request);

        Assert.False(result.Succeeded);

        Assert.Equal(
            EmployeeManagementErrorCodes.InvalidPermission ,
            result.ErrorCode);

        var employeeWasCreated =
            await dbContext.Users.AnyAsync(
                user =>
                    user.NormalizedEmail ==
                    userManager.NormalizeEmail(
                        request.Email));

        Assert.False(employeeWasCreated);
    }

    private static ServiceProvider
        CreateServiceProvider()
    {
        var services =
            new ServiceCollection();

        services.AddLogging();
        services.AddDataProtection();

        services.AddDbContext<MawasemDbContext>(
            options =>
            {
                options.UseInMemoryDatabase(
                    Guid.NewGuid().ToString("N"));

                options.ConfigureWarnings(
                    warnings =>
                        warnings.Ignore(
                            InMemoryEventId
                                .TransactionIgnoredWarning));
            });

        services
            .AddIdentityCore<ApplicationUser>(
                options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequiredUniqueChars = 1;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;

                    options.User.RequireUniqueEmail =
                        false;

                    options.Lockout.AllowedForNewUsers =
                        true;
                })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<MawasemDbContext>()
            .AddDefaultTokenProviders();

        services.AddSingleton(
            TimeProvider.System);

        services.AddScoped<
            EmployeeManagementService>();

        return services.BuildServiceProvider();
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

    private static ApplicationUser
        CreateDashboardUser(
            string email )
    {
        return new ApplicationUser
        {
            UserName = email ,
            Email = email ,
            EmailConfirmed = true ,
            FullNameAr = "مستخدم لوحة التحكم" ,
            FullNameEn = "Dashboard User" ,
            IsBlocked = false ,
            MustChangePassword = false ,
            LockoutEnabled = true
        };
    }
}