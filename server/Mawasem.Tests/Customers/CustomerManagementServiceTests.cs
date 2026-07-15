using Mawasem.Application.Features.Customers.Contracts.Requests;
using Mawasem.Application.Features.Customers.Models;
using Mawasem.Domain.Enums;
using Mawasem.Domain.Identity;
using Mawasem.Domain.Orders;
using Mawasem.Infrastructure.Customers;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mawasem.Tests.Customers;

public sealed class CustomerManagementServiceTests
{
    [Fact]
    public async Task GetListAsync_ReturnsOnlyCustomersWithOrderTotals()
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

        var adminRole =
            CreateRole(
                id: 2 ,
                SystemRoles.Admin);

        dbContext.Roles.AddRange(
            customerRole ,
            adminRole);

        var customer =
            CreateUser(
                id: 1 ,
                fullNameEn: "Customer Account" ,
                phoneNumber: "+201001111111");

        var dashboardUser =
            CreateUser(
                id: 2 ,
                fullNameEn: "Dashboard Account" ,
                phoneNumber: null);

        dbContext.Users.AddRange(
            customer ,
            dashboardUser);

        dbContext.UserRoles.AddRange(
            new IdentityUserRole<int>
            {
                UserId = customer.Id ,
                RoleId = customerRole.Id
            } ,
            new IdentityUserRole<int>
            {
                UserId = dashboardUser.Id ,
                RoleId = adminRole.Id
            });

        dbContext.Orders.AddRange(
            CreateOrder(
                id: 1 ,
                customer.Id ,
                "ORD-001" ,
                125m ,
                PaymentStatus.Paid ,
                OrderStatus.Delivered) ,
            CreateOrder(
                id: 2 ,
                customer.Id ,
                "ORD-002" ,
                75m ,
                PaymentStatus.Pending ,
                OrderStatus.Pending));

        await dbContext.SaveChangesAsync();

        var result =
            await service.GetListAsync(
                new GetCustomersRequest());

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Response);

        var item =
            Assert.Single(result.Response.Items);

        Assert.Equal(
            customer.Id ,
            item.Id);

        Assert.Equal(
            2 ,
            item.TotalOrders);

        Assert.Equal(
            125m ,
            item.TotalSpent);

        Assert.Equal(
            1 ,
            result.Response.TotalCount);
    }

    [Fact]
    public async Task GetListAsync_AppliesSearchAndBlockedFilters()
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

        dbContext.Roles.Add(customerRole);

        var activeCustomer =
            CreateUser(
                id: 10 ,
                fullNameEn: "Active Customer" ,
                phoneNumber: "+201002222222");

        var blockedCustomer =
            CreateUser(
                id: 11 ,
                fullNameEn: "Blocked Customer" ,
                phoneNumber: "+201003333333");

        blockedCustomer.IsBlocked = true;
        blockedCustomer.BlockedAt =
            DateTime.UtcNow;
        blockedCustomer.BlockedReason =
            "Test block reason.";

        dbContext.Users.AddRange(
            activeCustomer ,
            blockedCustomer);

        dbContext.UserRoles.AddRange(
            new IdentityUserRole<int>
            {
                UserId = activeCustomer.Id ,
                RoleId = customerRole.Id
            } ,
            new IdentityUserRole<int>
            {
                UserId = blockedCustomer.Id ,
                RoleId = customerRole.Id
            });

        await dbContext.SaveChangesAsync();

        var result =
            await service.GetListAsync(
                new GetCustomersRequest
                {
                    Search = "Blocked" ,
                    IsBlocked = true ,
                    PageNumber = 1 ,
                    PageSize = 20
                });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Response);

        var item =
            Assert.Single(result.Response.Items);

        Assert.Equal(
            blockedCustomer.Id ,
            item.Id);

        Assert.True(item.IsBlocked);
        Assert.Equal(
            1 ,
            result.Response.TotalCount);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCustomerDetailsAndRejectsDashboardUser()
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
                id: 20 ,
                SystemRoles.Customer);

        var adminRole =
            CreateRole(
                id: 21 ,
                SystemRoles.Admin);

        dbContext.Roles.AddRange(
            customerRole ,
            adminRole);

        var customer =
            CreateUser(
                id: 20 ,
                fullNameEn: "Detailed Customer" ,
                phoneNumber: "+201004444444");

        customer.BirthDate =
            new DateOnly(
                2000 ,
                1 ,
                1);

        customer.Gender =
            Gender.Male;

        var dashboardUser =
            CreateUser(
                id: 21 ,
                fullNameEn: "Not A Customer" ,
                phoneNumber: null);

        dbContext.Users.AddRange(
            customer ,
            dashboardUser);

        dbContext.UserRoles.AddRange(
            new IdentityUserRole<int>
            {
                UserId = customer.Id ,
                RoleId = customerRole.Id
            } ,
            new IdentityUserRole<int>
            {
                UserId = dashboardUser.Id ,
                RoleId = adminRole.Id
            });

        dbContext.Orders.AddRange(
            CreateOrder(
                id: 20 ,
                customer.Id ,
                "ORD-020" ,
                300m ,
                PaymentStatus.Paid ,
                OrderStatus.Delivered) ,
            CreateOrder(
                id: 21 ,
                customer.Id ,
                "ORD-021" ,
                100m ,
                PaymentStatus.Failed ,
                OrderStatus.Cancelled));

        await dbContext.SaveChangesAsync();

        var customerResult =
            await service.GetByIdAsync(
                customer.Id);

        Assert.True(customerResult.Succeeded);
        Assert.NotNull(customerResult.Response);

        Assert.Equal(
            customer.Id ,
            customerResult.Response.Id);

        Assert.Equal(
            2 ,
            customerResult.Response.TotalOrders);

        Assert.Equal(
            1 ,
            customerResult.Response.DeliveredOrders);

        Assert.Equal(
            300m ,
            customerResult.Response.TotalSpent);

        Assert.Equal(
            nameof(Gender.Male) ,
            customerResult.Response.Gender);

        var dashboardResult =
            await service.GetByIdAsync(
                dashboardUser.Id);

        Assert.False(dashboardResult.Succeeded);

        Assert.Equal(
            CustomerManagementErrorCodes.NotFound ,
            dashboardResult.ErrorCode);
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
        string fullNameEn ,
        string? phoneNumber )
    {
        var userName =
            phoneNumber
            ?? $"user-{id}@example.com";

        return new ApplicationUser
        {
            Id = id ,
            UserName = userName ,
            NormalizedUserName =
                userName.ToUpperInvariant() ,
            Email =
                phoneNumber is null
                    ? userName
                    : null ,
            NormalizedEmail =
                phoneNumber is null
                    ? userName.ToUpperInvariant()
                    : null ,
            PhoneNumber = phoneNumber ,
            FullNameAr = "مستخدم تجريبي" ,
            FullNameEn = fullNameEn ,
            IsBlocked = false ,
            MustChangePassword = false ,
            LockoutEnabled = true
        };
    }

    private static Order CreateOrder(
        int id ,
        int customerId ,
        string orderNumber ,
        decimal totalAmount ,
        PaymentStatus paymentStatus ,
        OrderStatus orderStatus )
    {
        return new Order
        {
            Id = id ,
            UserId = customerId ,
            CustomerNameAr = "عميل تجريبي" ,
            CustomerNameEn = "Test Customer" ,
            CustomerPhone = "+201000000000" ,
            OrderNumber = orderNumber ,
            OrderDate = DateTime.UtcNow ,
            SubTotal = totalAmount ,
            Discount = 0m ,
            DeliveryFee = 0m ,
            TotalAmount = totalAmount ,
            PaymentStatus = paymentStatus ,
            OrderStatus = orderStatus ,
            CreatedOn =
                DateTimeOffset.UtcNow
        };
    }
}