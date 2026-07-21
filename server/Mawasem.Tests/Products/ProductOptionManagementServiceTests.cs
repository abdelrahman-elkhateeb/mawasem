using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Models;
using Mawasem.Infrastructure.Persistence.Contexts;
using Mawasem.Infrastructure.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mawasem.Tests.Products;

public sealed class ProductOptionManagementServiceTests
{
    [Fact]
    public async Task CreateAndUpdateAsync_ManageOptionsAndValues()
    {
        await using var provider =
            CreateServiceProvider();

        await using var scope =
            provider.CreateAsyncScope();

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    ProductOptionManagementService>();

        var createOptionResult =
            await service.CreateAsync(
                new CreateProductOptionRequest
                {
                    NameAr = "  اللون  " ,
                    NameEn = "  Color  "
                });

        Assert.True(
            createOptionResult.Succeeded);

        Assert.NotNull(
            createOptionResult.Response);

        var createdOption =
            createOptionResult.Response;

        Assert.Equal(
            "اللون" ,
            createdOption.NameAr);

        Assert.Equal(
            "Color" ,
            createdOption.NameEn);

        Assert.Empty(
            createdOption.Values);

        var createValueResult =
            await service.CreateValueAsync(
                createdOption.Id ,
                new CreateProductOptionValueRequest
                {
                    ValueAr = "  أحمر  " ,
                    ValueEn = "  Red  "
                });

        Assert.True(
            createValueResult.Succeeded);

        Assert.NotNull(
            createValueResult.Response);

        var createdValue =
            createValueResult.Response;

        Assert.Equal(
            "أحمر" ,
            createdValue.ValueAr);

        Assert.Equal(
            "Red" ,
            createdValue.ValueEn);

        var updateOptionResult =
            await service.UpdateAsync(
                createdOption.Id ,
                new UpdateProductOptionRequest
                {
                    NameAr = "  لون المنتج  " ,
                    NameEn = "  Product Color  "
                });

        Assert.True(
            updateOptionResult.Succeeded);

        Assert.NotNull(
            updateOptionResult.Response);

        Assert.Equal(
            "لون المنتج" ,
            updateOptionResult.Response.NameAr);

        Assert.Equal(
            "Product Color" ,
            updateOptionResult.Response.NameEn);

        var updateValueResult =
            await service.UpdateValueAsync(
                createdOption.Id ,
                createdValue.Id ,
                new UpdateProductOptionValueRequest
                {
                    ValueAr = "  أحمر داكن  " ,
                    ValueEn = "  Dark Red  "
                });

        Assert.True(
            updateValueResult.Succeeded);

        Assert.NotNull(
            updateValueResult.Response);

        Assert.Equal(
            "أحمر داكن" ,
            updateValueResult.Response.ValueAr);

        Assert.Equal(
            "Dark Red" ,
            updateValueResult.Response.ValueEn);

        var getAllResult =
            await service.GetAllAsync();

        Assert.True(
            getAllResult.Succeeded);

        Assert.NotNull(
            getAllResult.Response);

        var returnedOption =
            Assert.Single(
                getAllResult.Response);

        Assert.Equal(
            createdOption.Id ,
            returnedOption.Id);

        Assert.Equal(
            "لون المنتج" ,
            returnedOption.NameAr);

        Assert.Equal(
            "Product Color" ,
            returnedOption.NameEn);

        var returnedValue =
            Assert.Single(
                returnedOption.Values);

        Assert.Equal(
            createdValue.Id ,
            returnedValue.Id);

        Assert.Equal(
            "أحمر داكن" ,
            returnedValue.ValueAr);

        Assert.Equal(
            "Dark Red" ,
            returnedValue.ValueEn);
    }

    [Fact]
    public async Task UpdateValueAsync_RejectsValueFromDifferentOption()
    {
        await using var provider =
            CreateServiceProvider();

        await using var scope =
            provider.CreateAsyncScope();

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    ProductOptionManagementService>();

        var colorResult =
            await service.CreateAsync(
                new CreateProductOptionRequest
                {
                    NameAr = "اللون" ,
                    NameEn = "Color"
                });

        var sizeResult =
            await service.CreateAsync(
                new CreateProductOptionRequest
                {
                    NameAr = "المقاس" ,
                    NameEn = "Size"
                });

        Assert.True(colorResult.Succeeded);
        Assert.NotNull(colorResult.Response);

        Assert.True(sizeResult.Succeeded);
        Assert.NotNull(sizeResult.Response);

        var redResult =
            await service.CreateValueAsync(
                colorResult.Response.Id ,
                new CreateProductOptionValueRequest
                {
                    ValueAr = "أحمر" ,
                    ValueEn = "Red"
                });

        Assert.True(redResult.Succeeded);
        Assert.NotNull(redResult.Response);

        var invalidUpdateResult =
            await service.UpdateValueAsync(
                sizeResult.Response.Id ,
                redResult.Response.Id ,
                new UpdateProductOptionValueRequest
                {
                    ValueAr = "كبير" ,
                    ValueEn = "Large"
                });

        Assert.False(
            invalidUpdateResult.Succeeded);

        Assert.Equal(
            ProductOptionManagementErrorCodes
                .OptionValueDoesNotBelongToOption ,
            invalidUpdateResult.ErrorCode);
    }

    private static ServiceProvider
        CreateServiceProvider()
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

        services.AddSingleton<TimeProvider>(
            TimeProvider.System);

        services.AddScoped<
            ProductOptionManagementService>();

        return services.BuildServiceProvider();
    }
}