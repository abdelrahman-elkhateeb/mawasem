using System.Security.Cryptography;
using System.Text;
using Mawasem.Application.Features.Carts.Models;
using Mawasem.Domain.Carts;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Carts;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Tests.Carts;

public sealed class CartServiceTests
{
    [Fact]
    public async Task CreateGuestAsync_StoresHashInsteadOfRawToken()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var result = await service.CreateGuestAsync();

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Response);
        Assert.Equal(64, result.Response.Token.Length);

        var cart = await dbContext.Carts.SingleAsync();
        var expectedHash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(result.Response.Token)));

        Assert.Equal(expectedHash, cart.GuestTokenHash);
        Assert.NotEqual(result.Response.Token, cart.GuestTokenHash);
    }

    [Fact]
    public async Task AddForGuestAsync_AddsAvailableVariant()
    {
        await using var dbContext = CreateDbContext();
        await SeedProductAsync(dbContext, stockQuantity: 5);

        var service = CreateService(dbContext);
        var guest = await service.CreateGuestAsync();

        var result = await service.AddForGuestAsync(
            guest.Response!.Token,
            productVariantId: 101,
            quantity: 2);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Response);
        Assert.True(result.Response.WasCreated);
        Assert.Equal(2, result.Response.Quantity);
        Assert.Equal(100m, result.Response.UnitPriceSnapshot);
        Assert.Equal(200m, result.Response.LineTotal);
    }

    [Fact]
    public async Task AddForGuestAsync_RejectsQuantityAboveStock()
    {
        await using var dbContext = CreateDbContext();
        await SeedProductAsync(dbContext, stockQuantity: 2);

        var service = CreateService(dbContext);
        var guest = await service.CreateGuestAsync();

        var result = await service.AddForGuestAsync(
            guest.Response!.Token,
            productVariantId: 101,
            quantity: 3);

        Assert.False(result.Succeeded);
        Assert.Equal(
            CartErrorCodes.InsufficientStock,
            result.ErrorCode);
        Assert.Empty(dbContext.CartItems);
    }

    [Fact]
    public async Task AddForCustomerAsync_AccumulatesExistingQuantity()
    {
        await using var dbContext = CreateDbContext();
        await SeedCustomerAsync(dbContext, userId: 1);
        await SeedProductAsync(dbContext, stockQuantity: 10);

        var service = CreateService(dbContext);

        var first = await service.AddForCustomerAsync(
            userId: 1,
            productVariantId: 101,
            quantity: 2);

        var second = await service.AddForCustomerAsync(
            userId: 1,
            productVariantId: 101,
            quantity: 3);

        Assert.True(first.Succeeded);
        Assert.True(second.Succeeded);
        Assert.False(second.Response!.WasCreated);
        Assert.Equal(5, second.Response.Quantity);
        Assert.Single(
            dbContext.CartItems.Where(item => !item.IsDeleted));
    }

    [Fact]
    public async Task UpdateForCustomerAsync_DoesNotExposeAnotherCustomersItem()
    {
        await using var dbContext = CreateDbContext();
        await SeedCustomerAsync(dbContext, userId: 1);
        await SeedCustomerAsync(dbContext, userId: 2);
        await SeedProductAsync(dbContext, stockQuantity: 10);

        var service = CreateService(dbContext);
        var added = await service.AddForCustomerAsync(
            userId: 1,
            productVariantId: 101,
            quantity: 2);

        var result = await service.UpdateForCustomerAsync(
            userId: 2,
            cartItemId: added.Response!.CartItemId,
            quantity: 1);

        Assert.False(result.Succeeded);
        Assert.Equal(
            CartErrorCodes.CartItemNotFound,
            result.ErrorCode);
    }

    [Fact]
    public async Task UpdateForGuestAsync_RefreshesPriceSnapshot()
    {
        await using var dbContext = CreateDbContext();
        await SeedProductAsync(dbContext, stockQuantity: 10);

        var service = CreateService(dbContext);
        var guest = await service.CreateGuestAsync();
        var added = await service.AddForGuestAsync(
            guest.Response!.Token,
            productVariantId: 101,
            quantity: 1);

        var product = await dbContext.Products.SingleAsync();
        product.CurrentPrice = 125m;
        await dbContext.SaveChangesAsync();

        var result = await service.UpdateForGuestAsync(
            guest.Response.Token,
            added.Response!.CartItemId,
            quantity: 2);

        Assert.True(result.Succeeded);
        Assert.Equal(125m, result.Response!.UnitPriceSnapshot);
        Assert.Equal(250m, result.Response.LineTotal);
    }

    [Fact]
    public async Task RemoveForGuestAsync_SoftDeletesOwnedItem()
    {
        await using var dbContext = CreateDbContext();
        await SeedProductAsync(dbContext, stockQuantity: 10);

        var service = CreateService(dbContext);
        var guest = await service.CreateGuestAsync();
        var added = await service.AddForGuestAsync(
            guest.Response!.Token,
            productVariantId: 101,
            quantity: 1);

        var result = await service.RemoveForGuestAsync(
            guest.Response.Token,
            added.Response!.CartItemId);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Response!.AffectedItemCount);

        var item = await dbContext.CartItems.SingleAsync();
        Assert.True(item.IsDeleted);
        Assert.NotNull(item.DeletedOn);
    }

    [Fact]
    public async Task ClearForCustomerAsync_SoftDeletesAllOwnedItems()
    {
        await using var dbContext = CreateDbContext();
        await SeedCustomerAsync(dbContext, userId: 1);
        await SeedProductAsync(dbContext, stockQuantity: 10);

        var service = CreateService(dbContext);
        await service.AddForCustomerAsync(
            userId: 1,
            productVariantId: 101,
            quantity: 2);

        var result = await service.ClearForCustomerAsync(userId: 1);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Response!.AffectedItemCount);
        Assert.All(
            dbContext.CartItems,
            item => Assert.True(item.IsDeleted));
    }

    [Fact]
    public async Task GetForGuestAsync_ReturnsCurrentTotalsAndWarnings()
    {
        await using var dbContext = CreateDbContext();
        await SeedProductAsync(dbContext, stockQuantity: 5);

        var service = CreateService(dbContext);
        var guest = await service.CreateGuestAsync();
        await service.AddForGuestAsync(
            guest.Response!.Token,
            productVariantId: 101,
            quantity: 3);

        var product = await dbContext.Products.SingleAsync();
        product.CurrentPrice = 120m;

        var variant = await dbContext.ProductVariants.SingleAsync();
        variant.StockQuantity = 2;

        await dbContext.SaveChangesAsync();

        var result = await service.GetForGuestAsync(
            guest.Response.Token);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Response);
        Assert.Equal(360m, result.Response.Subtotal);
        Assert.True(result.Response.HasWarnings);

        var item = Assert.Single(result.Response.Items);
        Assert.False(item.IsPurchasable);
        Assert.Contains(
            item.Warnings,
            warning =>
                warning.Code == CartWarningCodes.PriceChanged);
        Assert.Contains(
            item.Warnings,
            warning =>
                warning.Code == CartWarningCodes.InsufficientStock);
    }

    [Fact]
    public async Task MergeGuestIntoCustomerAsync_CombinesQuantitiesAndConsumesGuestCart()
    {
        await using var dbContext = CreateDbContext();
        await SeedCustomerAsync(dbContext, userId: 1);
        await SeedProductAsync(dbContext, stockQuantity: 10);

        var service = CreateService(dbContext);
        var guest = await service.CreateGuestAsync();

        await service.AddForGuestAsync(
            guest.Response!.Token,
            productVariantId: 101,
            quantity: 2);

        await service.AddForCustomerAsync(
            userId: 1,
            productVariantId: 101,
            quantity: 3);

        var result = await service.MergeGuestIntoCustomerAsync(
            userId: 1,
            guest.Response.Token);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Response!.MergedItemCount);

        var customerCart = await dbContext.Carts
            .Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == 1);

        Assert.Equal(
            5,
            Assert.Single(
                customerCart.Items.Where(item => !item.IsDeleted))
                .Quantity);

        var consumedGuestCart = await dbContext.Carts
            .SingleAsync(cart => cart.UserId == null);

        Assert.True(consumedGuestCart.IsDeleted);
    }

    [Fact]
    public async Task GuestOperations_RejectInvalidToken()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var result = await service.GetForGuestAsync("invalid");

        Assert.False(result.Succeeded);
        Assert.Equal(
            CartErrorCodes.InvalidGuestToken,
            result.ErrorCode);
    }

    [Fact]
    public async Task CustomerOperations_RejectBlockedCustomer()
    {
        await using var dbContext = CreateDbContext();
        await SeedCustomerAsync(
            dbContext,
            userId: 1,
            isBlocked: true);

        var service = CreateService(dbContext);

        var result = await service.GetForCustomerAsync(userId: 1);

        Assert.False(result.Succeeded);
        Assert.Equal(
            CartErrorCodes.AccountBlocked,
            result.ErrorCode);
    }

    private static MawasemDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<MawasemDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new MawasemDbContext(options);
    }

    private static CartService CreateService(
        MawasemDbContext dbContext)
    {
        return new CartService(
            dbContext,
            TimeProvider.System);
    }

    private static async Task SeedCustomerAsync(
        MawasemDbContext dbContext,
        int userId,
        bool isBlocked = false)
    {
        dbContext.Users.Add(
            new ApplicationUser
            {
                Id = userId,
                UserName = $"customer-{userId}",
                NormalizedUserName = $"CUSTOMER-{userId}",
                SecurityStamp = Guid.NewGuid().ToString(),
                IsBlocked = isBlocked
            });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedProductAsync(
        MawasemDbContext dbContext,
        int stockQuantity)
    {
        var product = new Product
        {
            Id = 100,
            OriginalPrice = 120m,
            CurrentPrice = 100m,
            IsPublished = true,
            Slug = "test-product",
            BrandId = 1,
            SeasonId = 1
        };

        product.Variants.Add(
            new ProductVariant
            {
                Id = 101,
                ProductId = product.Id,
                SKU = "TEST-101",
                OptionCombinationKey = "default",
                StockQuantity = stockQuantity,
                IsAvailable = true
            });

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
    }
}
