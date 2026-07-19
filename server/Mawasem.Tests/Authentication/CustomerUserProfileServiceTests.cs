using Mawasem.Application.Features.Authentication.Models;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Authentication;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mawasem.Tests.Authentication;

public sealed class CustomerUserProfileServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsAuthenticatedCustomerProfile()
    {
        await using var provider =
            CreateServiceProvider();

        await using var scope =
            provider.CreateAsyncScope();

        var user =
            await SeedUserAsync(
                scope.ServiceProvider ,
                userId: 100 ,
                roleId: 100 ,
                SystemRoles.Customer ,
                isBlocked: false);

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    CustomerUserProfileService>();

        var result =
            await service.GetAsync(
                user.Id);

        Assert.True(
            result.Succeeded);

        Assert.NotNull(
            result.User);

        Assert.Equal(
            user.Id ,
            result.User.Id);

        Assert.Equal(
            user.FullNameAr ,
            result.User.FullNameAr);

        Assert.Equal(
            user.FullNameEn ,
            result.User.FullNameEn);

        Assert.Equal(
            user.PhoneNumber ,
            result.User.PhoneNumber);

        Assert.Equal(
            user.Email ,
            result.User.Email);

        Assert.Equal(
            SystemRoles.Customer ,
            Assert.Single(
                result.User.Roles));
    }

    [Fact]
    public async Task GetAsync_RejectsBlockedCustomer()
    {
        await using var provider =
            CreateServiceProvider();

        await using var scope =
            provider.CreateAsyncScope();

        var user =
            await SeedUserAsync(
                scope.ServiceProvider ,
                userId: 200 ,
                roleId: 200 ,
                SystemRoles.Customer ,
                isBlocked: true);

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    CustomerUserProfileService>();

        var result =
            await service.GetAsync(
                user.Id);

        Assert.False(
            result.Succeeded);

        Assert.Null(
            result.User);

        Assert.Equal(
            AuthenticationErrorCodes.AccountBlocked ,
            result.ErrorCode);
    }

    [Fact]
    public async Task GetAsync_RejectsDashboardAccount()
    {
        await using var provider =
            CreateServiceProvider();

        await using var scope =
            provider.CreateAsyncScope();

        var user =
            await SeedUserAsync(
                scope.ServiceProvider ,
                userId: 300 ,
                roleId: 300 ,
                SystemRoles.Admin ,
                isBlocked: false);

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    CustomerUserProfileService>();

        var result =
            await service.GetAsync(
                user.Id);

        Assert.False(
            result.Succeeded);

        Assert.Null(
            result.User);

        Assert.Equal(
            AuthenticationErrorCodes.InvalidCredentials ,
            result.ErrorCode);
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
            });

        services
            .AddIdentityCore<ApplicationUser>(
                options =>
                {
                    options.User.RequireUniqueEmail =
                        false;
                })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<MawasemDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<
            CustomerUserProfileService>();

        return services.BuildServiceProvider();
    }

    private static async Task<ApplicationUser>
        SeedUserAsync(
            IServiceProvider serviceProvider ,
            int userId ,
            int roleId ,
            string roleName ,
            bool isBlocked )
    {
        var dbContext =
            serviceProvider.GetRequiredService<
                MawasemDbContext>();

        var normalizedRoleName =
            roleName.ToUpperInvariant();

        var role =
            new ApplicationRole
            {
                Id = roleId ,
                Name = roleName ,
                NormalizedName = normalizedRoleName
            };

        var user =
            new ApplicationUser
            {
                Id = userId ,
                UserName =
                    $"user-{userId}@example.com" ,

                NormalizedUserName =
                    $"USER-{userId}@EXAMPLE.COM" ,

                Email =
                    $"user-{userId}@example.com" ,

                NormalizedEmail =
                    $"USER-{userId}@EXAMPLE.COM" ,

                EmailConfirmed = true ,

                PhoneNumber =
                    $"01000000{userId}" ,

                PhoneNumberConfirmed = true ,

                FullNameAr =
                    $"مستخدم {userId}" ,

                FullNameEn =
                    $"User {userId}" ,

                IsBlocked = isBlocked ,
                MustChangePassword = false ,
                LockoutEnabled = true
            };

        dbContext.Roles.Add(role);

        dbContext.Users.Add(user);

        dbContext.UserRoles.Add(
            new IdentityUserRole<int>
            {
                UserId = user.Id ,
                RoleId = role.Id
            });

        await dbContext.SaveChangesAsync();

        return user;
    }
}