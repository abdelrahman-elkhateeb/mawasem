using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Contracts.Responses;
using Mawasem.Application.Features.Products.Interfaces;
using Mawasem.Application.Features.Products.Models;
using Mawasem.Domain.Catalog;
using Mawasem.Domain.Common.ValueObjects;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Products;

public sealed class ProductOptionManagementService
    : IProductOptionManagementService
{
    private readonly MawasemDbContext _dbContext;

    public ProductOptionManagementService(
        MawasemDbContext dbContext )
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public async Task<
        ProductManagementResult<
            IReadOnlyCollection<ProductOptionResponse>>>
        GetAllAsync(
            CancellationToken cancellationToken = default )
    {
        var options =
            await _dbContext
                .Set<ProductOption>()
                .AsNoTracking()
                .Include(x => x.Values)
                .OrderBy(x => x.Name.English)
                .ThenBy(x => x.Id)
                .ToArrayAsync(cancellationToken);

        IReadOnlyCollection<ProductOptionResponse> response =
            options
                .Select(MapOption)
                .ToArray();

        return ProductManagementResult<
            IReadOnlyCollection<ProductOptionResponse>>
            .Success(response);
    }

    public async Task<
        ProductManagementResult<ProductOptionResponse>>
        CreateAsync(
            CreateProductOptionRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationError =
            ValidateLocalizedText(
                request.NameAr ,
                request.NameEn ,
                "option name");

        if ( validationError is not null )
        {
            return ProductManagementResult<
                ProductOptionResponse>.Failure(
                    ProductManagementErrorCodes.InvalidRequest ,
                    validationError);
        }

        var option =
            new ProductOption
            {
                Name =
                    new LocalizedText(
                        request.NameEn.Trim() ,
                        request.NameAr.Trim())
            };

        _dbContext
            .Set<ProductOption>()
            .Add(option);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return ProductManagementResult<
            ProductOptionResponse>.Success(
                MapOption(option));
    }

    public async Task<
        ProductManagementResult<ProductOptionResponse>>
        UpdateAsync(
            int optionId ,
            UpdateProductOptionRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( optionId <= 0 )
        {
            return ProductManagementResult<
                ProductOptionResponse>.Failure(
                    ProductManagementErrorCodes.InvalidRequest ,
                    "The product option identifier is invalid.");
        }

        var validationError =
            ValidateLocalizedText(
                request.NameAr ,
                request.NameEn ,
                "option name");

        if ( validationError is not null )
        {
            return ProductManagementResult<
                ProductOptionResponse>.Failure(
                    ProductManagementErrorCodes.InvalidRequest ,
                    validationError);
        }

        var option =
            await _dbContext
                .Set<ProductOption>()
                .Include(x => x.Values)
                .SingleOrDefaultAsync(
                    x => x.Id == optionId ,
                    cancellationToken);

        if ( option is null )
        {
            return ProductManagementResult<
                ProductOptionResponse>.Failure(
                    ProductOptionManagementErrorCodes.OptionNotFound ,
                    "The requested product option was not found.");
        }

        option.Name.Update(
            request.NameEn.Trim() ,
            request.NameAr.Trim());

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return ProductManagementResult<
            ProductOptionResponse>.Success(
                MapOption(option));
    }

    public async Task<
        ProductManagementResult<ProductOptionValueResponse>>
        CreateValueAsync(
            int optionId ,
            CreateProductOptionValueRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( optionId <= 0 )
        {
            return ProductManagementResult<
                ProductOptionValueResponse>.Failure(
                    ProductManagementErrorCodes.InvalidRequest ,
                    "The product option identifier is invalid.");
        }

        var validationError =
            ValidateLocalizedText(
                request.ValueAr ,
                request.ValueEn ,
                "option value");

        if ( validationError is not null )
        {
            return ProductManagementResult<
                ProductOptionValueResponse>.Failure(
                    ProductManagementErrorCodes.InvalidRequest ,
                    validationError);
        }

        var optionExists =
            await _dbContext
                .Set<ProductOption>()
                .AnyAsync(
                    x => x.Id == optionId ,
                    cancellationToken);

        if ( !optionExists )
        {
            return ProductManagementResult<
                ProductOptionValueResponse>.Failure(
                    ProductOptionManagementErrorCodes.OptionNotFound ,
                    "The requested product option was not found.");
        }

        var optionValue =
            new ProductOptionValue
            {
                ProductOptionId = optionId ,

                Value =
                    new LocalizedText(
                        request.ValueEn.Trim() ,
                        request.ValueAr.Trim())
            };

        _dbContext
            .Set<ProductOptionValue>()
            .Add(optionValue);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return ProductManagementResult<
            ProductOptionValueResponse>.Success(
                MapValue(optionValue));
    }

    public async Task<
        ProductManagementResult<ProductOptionValueResponse>>
        UpdateValueAsync(
            int optionId ,
            int valueId ,
            UpdateProductOptionValueRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( optionId <= 0 ||
             valueId <= 0 )
        {
            return ProductManagementResult<
                ProductOptionValueResponse>.Failure(
                    ProductManagementErrorCodes.InvalidRequest ,
                    "The product option value identifiers are invalid.");
        }

        var validationError =
            ValidateLocalizedText(
                request.ValueAr ,
                request.ValueEn ,
                "option value");

        if ( validationError is not null )
        {
            return ProductManagementResult<
                ProductOptionValueResponse>.Failure(
                    ProductManagementErrorCodes.InvalidRequest ,
                    validationError);
        }

        var optionValue =
            await _dbContext
                .Set<ProductOptionValue>()
                .SingleOrDefaultAsync(
                    x => x.Id == valueId ,
                    cancellationToken);

        if ( optionValue is null )
        {
            return ProductManagementResult<
                ProductOptionValueResponse>.Failure(
                    ProductOptionManagementErrorCodes.OptionValueNotFound ,
                    "The requested product option value was not found.");
        }

        if ( optionValue.ProductOptionId != optionId )
        {
            return ProductManagementResult<
                ProductOptionValueResponse>.Failure(
                    ProductOptionManagementErrorCodes
                        .OptionValueDoesNotBelongToOption ,
                    "The requested value does not belong to the specified product option.");
        }

        optionValue.Value.Update(
            request.ValueEn.Trim() ,
            request.ValueAr.Trim());

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return ProductManagementResult<
            ProductOptionValueResponse>.Success(
                MapValue(optionValue));
    }

    private static string? ValidateLocalizedText(
        string arabic ,
        string english ,
        string fieldName )
    {
        if ( string.IsNullOrWhiteSpace(arabic) )
        {
            return $"The Arabic {fieldName} is required.";
        }

        if ( string.IsNullOrWhiteSpace(english) )
        {
            return $"The English {fieldName} is required.";
        }

        return null;
    }

    private static ProductOptionResponse MapOption(
        ProductOption option )
    {
        return new ProductOptionResponse
        {
            Id = option.Id ,
            NameAr = option.Name.Arabic ,
            NameEn = option.Name.English ,

            Values =
                option.Values
                    .OrderBy(x => x.Value.English)
                    .ThenBy(x => x.Id)
                    .Select(MapValue)
                    .ToArray()
        };
    }

    private static ProductOptionValueResponse MapValue(
        ProductOptionValue optionValue )
    {
        return new ProductOptionValueResponse
        {
            Id = optionValue.Id ,
            ValueAr = optionValue.Value.Arabic ,
            ValueEn = optionValue.Value.English
        };
    }
}