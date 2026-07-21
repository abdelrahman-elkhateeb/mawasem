using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Contracts.Responses;
using Mawasem.Application.Features.Products.Interfaces;
using Mawasem.Application.Features.Products.Models;
using Mawasem.Domain.Catalog;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;

namespace Mawasem.Infrastructure.Products;

public sealed class ProductVariantManagementService
    : IProductVariantManagementService
{
    private const int SqlServerRowVersionLength = 8;

    private const string CombinationIndexName =
        "IX_ProductVariants_ProductId_OptionCombinationKey";

    private readonly MawasemDbContext _dbContext;

    private readonly TimeProvider _timeProvider;

    public ProductVariantManagementService(
        MawasemDbContext dbContext ,
        TimeProvider timeProvider )
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<
        ProductManagementResult<
            IReadOnlyCollection<ProductVariantResponse>>>
        GetByProductIdAsync(
            int productId ,
            CancellationToken cancellationToken = default )
    {
        if ( productId <= 0 )
        {
            return ProductManagementResult<
                IReadOnlyCollection<ProductVariantResponse>>
                .Failure(
                    ProductManagementErrorCodes.InvalidRequest ,
                    "Select a valid product.");
        }

        if ( !await ProductExistsAsync(
                productId ,
                cancellationToken) )
        {
            return ProductManagementResult<
                IReadOnlyCollection<ProductVariantResponse>>
                .Failure(
                    ProductVariantManagementErrorCodes.ProductNotFound ,
                    "The requested product was not found.");
        }

        var variants =
            await CreateVariantQuery(productId)
                .OrderBy(x => x.Id)
                .ToArrayAsync(cancellationToken);

        IReadOnlyCollection<ProductVariantResponse> response =
            variants
                .Select(MapVariant)
                .ToArray();

        return ProductManagementResult<
            IReadOnlyCollection<ProductVariantResponse>>
            .Success(response);
    }

    public async Task<
        ProductManagementResult<ProductVariantResponse>>
        CreateAsync(
            int productId ,
            CreateProductVariantRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( productId <= 0 )
        {
            return InvalidVariantRequest(
                "Select a valid product.");
        }

        if ( request.OptionValueIds is null )
        {
            return InvalidVariantRequest(
                "Option value selections are required.");
        }

        if ( request.OptionValueIds.Any(x => x <= 0) )
        {
            return InvalidVariantRequest(
                "Select valid product option values.");
        }

        var selectedValueIds =
            request.OptionValueIds
                .ToArray();

        if ( selectedValueIds.Length !=
            selectedValueIds.Distinct().Count() )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes
                        .DuplicateOptionValueSelection ,
                    "An option value cannot be selected more than once.");
        }

        if ( !await ProductExistsAsync(
                productId ,
                cancellationToken) )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes.ProductNotFound ,
                    "The requested product was not found.");
        }

        var optionValues =
            await _dbContext
                .Set<ProductOptionValue>()
                .AsNoTracking()
                .Include(x => x.ProductOption)
                .Where(
                    x =>
                        selectedValueIds.Contains(x.Id) &&
                        !x.IsDeleted &&
                        !x.ProductOption.IsDeleted)
                .ToArrayAsync(cancellationToken);

        if ( optionValues.Length != selectedValueIds.Length )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes
                        .OptionValueNotFound ,
                    "One or more selected option values were not found.");
        }

        var selectedOptionIds =
            optionValues
                .Select(x => x.ProductOptionId)
                .OrderBy(x => x)
                .ToArray();

        if ( selectedOptionIds.Length !=
            selectedOptionIds.Distinct().Count() )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes
                        .MultipleValuesForSameOption ,
                    "Select only one value for each product option.");
        }

        var structureIsConsistent =
            await HasConsistentOptionStructureAsync(
                productId ,
                selectedOptionIds ,
                excludedVariantId: null ,
                cancellationToken);

        if ( !structureIsConsistent )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes
                        .InconsistentOptionStructure ,
                    "All available variants for a product must use the same option types.");
        }

        var combinationKey =
            CreateCombinationKey(selectedValueIds);

        var combinationExists =
            await _dbContext
                .Set<ProductVariant>()
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.ProductId == productId &&
                        x.OptionCombinationKey ==
                            combinationKey ,
                    cancellationToken);

        if ( combinationExists )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes
                        .CombinationAlreadyExists ,
                    "A variant with this option combination already exists.");
        }

        var now =
            _timeProvider.GetUtcNow();

        var variant =
            new ProductVariant
            {
                ProductId = productId ,
                SKU = GenerateSku(productId) ,
                OptionCombinationKey = combinationKey ,
                StockQuantity = 0 ,
                IsAvailable = true ,
                CreatedOn = now ,
                IsDeleted = false
            };

        foreach ( var optionValueId in selectedValueIds )
        {
            variant.Options.Add(
                new ProductVariantOption
                {
                    ProductOptionValueId = optionValueId ,
                    CreatedOn = now ,
                    IsDeleted = false
                });
        }

        _dbContext
            .Set<ProductVariant>()
            .Add(variant);

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch ( DbUpdateException exception )
            when ( IsCombinationConstraintViolation(
                exception) )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes
                        .CombinationAlreadyExists ,
                    "A variant with this option combination already exists.");
        }

        return await CreateVariantSuccessAsync(
            productId ,
            variant.Id ,
            cancellationToken);
    }

    public async Task<
        ProductManagementResult<ProductVariantResponse>>
        UpdateAvailabilityAsync(
            int productId ,
            int variantId ,
            UpdateProductVariantAvailabilityRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( productId <= 0 ||
            variantId <= 0 )
        {
            return InvalidVariantRequest(
                "The product variant request is invalid.");
        }

        if ( !await ProductExistsAsync(
                productId ,
                cancellationToken) )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes.ProductNotFound ,
                    "The requested product was not found.");
        }

        var variant =
            await _dbContext
                .Set<ProductVariant>()
                .SingleOrDefaultAsync(
                    x =>
                        x.Id == variantId &&
                        x.ProductId == productId &&
                        !x.IsDeleted ,
                    cancellationToken);

        if ( variant is null )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes.VariantNotFound ,
                    "The requested product variant was not found.");
        }

        if ( request.IsAvailable )
        {
            var selectedOptionIds =
                await _dbContext
                    .Set<ProductVariantOption>()
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.ProductVariantId == variantId &&
                            !x.IsDeleted)
                    .Select(
                        x =>
                            x.ProductOptionValue
                                .ProductOptionId)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToArrayAsync(cancellationToken);

            var structureIsConsistent =
                await HasConsistentOptionStructureAsync(
                    productId ,
                    selectedOptionIds ,
                    variantId ,
                    cancellationToken);

            if ( !structureIsConsistent )
            {
                return ProductManagementResult<
                    ProductVariantResponse>.Failure(
                        ProductVariantManagementErrorCodes
                            .InconsistentOptionStructure ,
                        "This variant does not use the same option types as the other available variants.");
            }
        }

        variant.IsAvailable =
            request.IsAvailable;

        variant.LastModifiedOn =
            _timeProvider.GetUtcNow();

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await CreateVariantSuccessAsync(
            productId ,
            variantId ,
            cancellationToken);
    }

    public async Task<
        ProductManagementResult<ProductVariantResponse>>
        UpdateStockAsync(
            int productId ,
            int variantId ,
            UpdateProductVariantStockRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( productId <= 0 ||
            variantId <= 0 )
        {
            return InvalidVariantRequest(
                "The product variant stock request is invalid.");
        }

        if ( request.StockQuantity < 0 )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes
                        .StockQuantityCannotBeNegative ,
                    "Stock quantity cannot be negative.");
        }

        if ( !TryDecodeRowVersion(
                request.RowVersion ,
                out var expectedRowVersion) )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes
                        .InvalidRowVersion ,
                    "The supplied row version is invalid.");
        }

        if ( !await ProductExistsAsync(
                productId ,
                cancellationToken) )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes.ProductNotFound ,
                    "The requested product was not found.");
        }

        var variant =
            await _dbContext
                .Set<ProductVariant>()
                .SingleOrDefaultAsync(
                    x =>
                        x.Id == variantId &&
                        x.ProductId == productId &&
                        !x.IsDeleted ,
                    cancellationToken);

        if ( variant is null )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes.VariantNotFound ,
                    "The requested product variant was not found.");
        }

        _dbContext
            .Entry(variant)
            .Property(x => x.RowVersion)
            .OriginalValue = expectedRowVersion;

        variant.StockQuantity =
            request.StockQuantity;

        variant.LastModifiedOn =
            _timeProvider.GetUtcNow();

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch ( DbUpdateConcurrencyException )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes
                        .StockConcurrencyConflict ,
                    "The stock was changed by another user. Refresh the variant and try again.");
        }

        return await CreateVariantSuccessAsync(
            productId ,
            variantId ,
            cancellationToken);
    }

    private IQueryable<ProductVariant> CreateVariantQuery(
        int productId )
    {
        return _dbContext
            .Set<ProductVariant>()
            .AsNoTracking()
            .Where(
                x =>
                    x.ProductId == productId &&
                    !x.IsDeleted)
            .Include(
                x =>
                    x.Options.Where(
                        option => !option.IsDeleted))
            .ThenInclude(
                x => x.ProductOptionValue)
            .ThenInclude(
                x => x.ProductOption);
    }

    private async Task<bool> ProductExistsAsync(
        int productId ,
        CancellationToken cancellationToken )
    {
        return await _dbContext
            .Set<Product>()
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == productId &&
                    !x.IsDeleted ,
                cancellationToken);
    }

    private async Task<bool>
        HasConsistentOptionStructureAsync(
            int productId ,
            IReadOnlyCollection<int> selectedOptionIds ,
            int? excludedVariantId ,
            CancellationToken cancellationToken )
    {
        var existingVariants =
            await _dbContext
                .Set<ProductVariant>()
                .AsNoTracking()
                .Where(
                    x =>
                        x.ProductId == productId &&
                        !x.IsDeleted &&
                        x.IsAvailable &&
                        ( !excludedVariantId.HasValue ||
                          x.Id != excludedVariantId.Value ))
                .Include(
                    x =>
                        x.Options.Where(
                            option => !option.IsDeleted))
                .ThenInclude(
                    x => x.ProductOptionValue)
                .ToArrayAsync(cancellationToken);

        var normalizedSelectedOptionIds =
            selectedOptionIds
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

        foreach ( var existingVariant in existingVariants )
        {
            var existingOptionIds =
                existingVariant.Options
                    .Select(
                        x =>
                            x.ProductOptionValue
                                .ProductOptionId)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToArray();

            if ( !existingOptionIds.SequenceEqual(
                    normalizedSelectedOptionIds) )
            {
                return false;
            }
        }

        return true;
    }

    private async Task<
        ProductManagementResult<ProductVariantResponse>>
        CreateVariantSuccessAsync(
            int productId ,
            int variantId ,
            CancellationToken cancellationToken )
    {
        var variant =
            await CreateVariantQuery(productId)
                .SingleOrDefaultAsync(
                    x => x.Id == variantId ,
                    cancellationToken);

        if ( variant is null )
        {
            return ProductManagementResult<
                ProductVariantResponse>.Failure(
                    ProductVariantManagementErrorCodes.VariantNotFound ,
                    "The product variant was not found after saving.");
        }

        return ProductManagementResult<
            ProductVariantResponse>.Success(
                MapVariant(variant));
    }

    private static ProductVariantResponse MapVariant(
        ProductVariant variant )
    {
        return new ProductVariantResponse
        {
            Id = variant.Id ,
            ProductId = variant.ProductId ,
            SKU = variant.SKU ,
            StockQuantity = variant.StockQuantity ,
            IsAvailable = variant.IsAvailable ,

            CanPurchase =
                variant.IsAvailable &&
                variant.StockQuantity > 0 ,

            RowVersion =
                Convert.ToBase64String(
                    variant.RowVersion) ,

            Options =
                variant.Options
                    .Where(x => !x.IsDeleted)
                    .OrderBy(
                        x =>
                            x.ProductOptionValue
                                .ProductOption
                                .Name
                                .English)
                    .ThenBy(
                        x =>
                            x.ProductOptionValue
                                .ProductOptionId)
                    .Select(
                        x =>
                            new ProductVariantOptionResponse
                            {
                                OptionId =
                                    x.ProductOptionValue
                                        .ProductOptionId ,

                                OptionNameAr =
                                    x.ProductOptionValue
                                        .ProductOption
                                        .Name
                                        .Arabic ,

                                OptionNameEn =
                                    x.ProductOptionValue
                                        .ProductOption
                                        .Name
                                        .English ,

                                ValueId =
                                    x.ProductOptionValueId ,

                                ValueAr =
                                    x.ProductOptionValue
                                        .Value
                                        .Arabic ,

                                ValueEn =
                                    x.ProductOptionValue
                                        .Value
                                        .English
                            })
                    .ToArray()
        };
    }

    private static ProductManagementResult<
        ProductVariantResponse> InvalidVariantRequest(
            string errorMessage )
    {
        return ProductManagementResult<
            ProductVariantResponse>.Failure(
                ProductManagementErrorCodes.InvalidRequest ,
                errorMessage);
    }

    private static string CreateCombinationKey(
        IReadOnlyCollection<int> optionValueIds )
    {
        if ( optionValueIds.Count == 0 )
        {
            return "DEFAULT";
        }

        return string.Join(
            "|" ,
            optionValueIds
                .OrderBy(x => x)
                .Select(
                    x =>
                        x.ToString(
                            CultureInfo.InvariantCulture)));
    }

    private static string GenerateSku(
        int productId )
    {
        var randomPart =
            Convert.ToHexString(
                RandomNumberGenerator.GetBytes(4));

        return FormattableString.Invariant(
            $"MWS-P{productId:D6}-{randomPart}");
    }

    private static bool TryDecodeRowVersion(
        string rowVersion ,
        out byte[] decodedRowVersion )
    {
        decodedRowVersion =
            Array.Empty<byte>();

        if ( string.IsNullOrWhiteSpace(rowVersion) )
        {
            return false;
        }

        try
        {
            decodedRowVersion =
                Convert.FromBase64String(
                    rowVersion.Trim());

            return decodedRowVersion.Length ==
                SqlServerRowVersionLength;
        }
        catch ( FormatException )
        {
            return false;
        }
    }

    private static bool IsCombinationConstraintViolation(
        DbUpdateException exception )
    {
        return exception.InnerException is SqlException
        {
            Number: 2601 or 2627
        } sqlException &&
        sqlException.Message.Contains(
            CombinationIndexName ,
            StringComparison.OrdinalIgnoreCase);
    }
}