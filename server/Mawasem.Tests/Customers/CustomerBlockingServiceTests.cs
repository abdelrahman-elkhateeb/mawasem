using Mawasem.Application.Features.Customers.Contracts.Requests;
using Mawasem.Application.Features.Customers.Models;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Customers;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mawasem.Tests.Customers;

public sealed class CustomerBlockingServiceTests
{
    [Fact]
    public async Task BlockAsync_BlocksCustomerAndRevokesActiveRefreshTokens()
    {
        await using var provider =
            CreateServiceProvider();

        await using var scope =
            provider.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var service =
            scope.ServiceProvider
                .GetRequiredService<CustomerManagementService>();

        var customerRole =
            CreateRole(
                id: 1 ,
                SystemRoles.Customer);

        var customer =
            CreateUser(
                id: 1 ,
                "Blocked Customer");

        dbContext.Roles.Add(customerRole);
        dbContext.Users.Add(customer);

        dbContext.UserRoles.Add(
            new IdentityUserRole<int>
            {
                UserId = customer.Id ,
                RoleId = customerRole.Id
            });

        var now =
            DateTime.UtcNow;

        var activeToken =
            new RefreshToken
            {
                Id = 1 ,
                UserId = customer.Id ,
                TokenHash = "active-token-hash" ,
                CreatedAtUtc =
                    now.AddDays(-1) ,
                ExpiresAtUtc =
                    now.AddDays(1)
            };

        var expiredToken =
            new RefreshToken
            {
                Id = 2 ,
                UserId = customer.Id ,
                TokenHash = "expired-token-hash" ,
                CreatedAtUtc =
                    now.AddDays(-2) ,
                ExpiresAtUtc =
                    now.AddDays(-1)
            };

        dbContext.RefreshTokens.AddRange(
            activeToken ,
            expiredToken);

        await dbContext.SaveChangesAsync();

        var result =
            await service.BlockAsync(
                customer.Id ,
                new BlockCustomerRequest
                {
                    Reason =
                        "Repeated policy violations."
                } ,
                "127.0.0.1");

        Assert.True(result.Succeeded);

        dbContext.ChangeTracker.Clear();

        var blockedCustomer =
            await dbContext.Users
                .SingleAsync(user =>
                    user.Id == customer.Id);

        Assert.True(blockedCustomer.IsBlocked);
        Assert.NotNull(blockedCustomer.BlockedAt);

        Assert.Equal(
            "Repeated policy violations." ,
            blockedCustomer.BlockedReason);

        var revokedActiveToken =
            await dbContext.RefreshTokens
                .SingleAsync(refreshToken =>
                    refreshToken.Id ==
                    activeToken.Id);

        Assert.NotNull(
            revokedActiveToken.RevokedAtUtc);

        Assert.Equal(
            "127.0.0.1" ,
            revokedActiveToken.RevokedByIp);

        Assert.Equal(
            "Customer account blocked by an administrator." ,
            revokedActiveToken.RevocationReason);

        var unchangedExpiredToken =
            await dbContext.RefreshTokens
                .SingleAsync(refreshToken =>
                    refreshToken.Id ==
                    expiredToken.Id);

        Assert.Null(
            unchangedExpiredToken.RevokedAtUtc);
    }

    [Fact]
    public async Task UnblockAsync_ClearsBlockAndLockoutState()
    {
        await using var provider =
            CreateServiceProvider();

        await using var scope =
            provider.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var service =
            scope.ServiceProvider
                .GetRequiredService<CustomerManagementService>();

        var customerRole =
            CreateRole(
                id: 10 ,
                SystemRoles.Customer);

        var customer =
            CreateUser(
                id: 10 ,
                "Locked Customer");

        customer.IsBlocked = true;
        customer.BlockedAt =
            DateTime.UtcNow.AddDays(-1);
        customer.BlockedReason =
            "Previous block reason.";
        customer.LockoutEnd =
            DateTimeOffset.UtcNow.AddHours(1);
        customer.AccessFailedCount = 5;

        dbContext.Roles.Add(customerRole);
        dbContext.Users.Add(customer);

        dbContext.UserRoles.Add(
            new IdentityUserRole<int>
            {
                UserId = customer.Id ,
                RoleId = customerRole.Id
            });

        await dbContext.SaveChangesAsync();

        var result =
            await service.UnblockAsync(
                customer.Id);

        Assert.True(result.Succeeded);

        dbContext.ChangeTracker.Clear();

        var unblockedCustomer =
            await dbContext.Users
                .SingleAsync(user =>
                    user.Id == customer.Id);

        Assert.False(unblockedCustomer.IsBlocked);
        Assert.Null(unblockedCustomer.BlockedAt);
        Assert.Null(unblockedCustomer.BlockedReason);
        Assert.Null(unblockedCustomer.LockoutEnd);

        Assert.Equal(
            0 ,
            unblockedCustomer.AccessFailedCount);
    }

    [Fact]
    public async Task BlockAsync_DoesNotTreatDashboardAccountAsCustomer()
    {
        await using var provider =
            CreateServiceProvider();

        await using var scope =
            provider.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var service =
            scope.ServiceProvider
                .GetRequiredService<CustomerManagementService>();

        var adminRole =
            CreateRole(
                id: 20 ,
                SystemRoles.Admin);

        var dashboardUser =
            CreateUser(
                id: 20 ,
                "Dashboard Account");

        dbContext.Roles.Add(adminRole);
        dbContext.Users.Add(dashboardUser);

        dbContext.UserRoles.Add(
            new IdentityUserRole<int>
            {
                UserId = dashboardUser.Id ,
                RoleId = adminRole.Id
            });

        await dbContext.SaveChangesAsync();

        var result =
            await service.BlockAsync(
                dashboardUser.Id ,
                new BlockCustomerRequest
                {
                    Reason =
                        "This operation must be rejected."
                } ,
                null);

        Assert.False(result.Succeeded);

        Assert.Equal(
            CustomerManagementErrorCodes.NotFound ,
            result.ErrorCode);

        dbContext.ChangeTracker.Clear();

        var unchangedUser =
            await dbContext.Users
                .SingleAsync(user =>
                    user.Id ==
                    dashboardUser.Id);

        Assert.False(unchangedUser.IsBlocked);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services =
            new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<MawasemDbContext>(
            options =>
            {
                options.UseInMemoryDatabase(
                    Guid.NewGuid().ToString("N"));
            });

        services.AddScoped<CustomerManagementService>();

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

    private static ApplicationUser CreateUser(
        int id ,
        string fullNameEn )
    {
        var phoneNumber =
            $"+20100{id:D7}";

        return new ApplicationUser
        {
            Id = id ,
            UserName = phoneNumber ,
            NormalizedUserName =
                phoneNumber.ToUpperInvariant() ,
            PhoneNumber = phoneNumber ,
            FullNameAr = "عميل تجريبي" ,
            FullNameEn = fullNameEn ,
            IsBlocked = false ,
            MustChangePassword = false ,
            LockoutEnabled = true
        };
    }
}