using Mawasem.Application.Features.Categories.Contracts.Requests;
using Mawasem.Application.Features.Categories.Contracts.Responses;
using Mawasem.Application.Features.Categories.Models;
using Mawasem.Domain.Catalog;
using Mawasem.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Mawasem.Infrastructure.Categories;

public sealed partial class CategoryManagementService
{
    public async Task<CategoryManagementResult<CategoryResponse>>
        CreateAsync(
            int actorUserId ,
            CreateCategoryRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( actorUserId <= 0 )
        {
            return CategoryManagementResult<CategoryResponse>
                .Failure(
                    CategoryManagementErrorCodes.InvalidRequest ,
                    "The authenticated dashboard account is invalid.");
        }

        if ( !TryNormalizeNames(
                request.NameEn ,
                request.NameAr ,
                out var nameEn ,
                out var nameAr ,
                out var validationError) )
        {
            return CategoryManagementResult<CategoryResponse>
                .Failure(
                    CategoryManagementErrorCodes.InvalidRequest ,
                    validationError);
        }

        if ( await HasDuplicateNameAsync(
                nameEn ,
                nameAr ,
                excludedCategoryId: null ,
                cancellationToken) )
        {
            return CategoryManagementResult<CategoryResponse>
                .Failure(
                    CategoryManagementErrorCodes.DuplicateName ,
                    "A category with the same Arabic or English name already exists.");
        }

        var now =
            _timeProvider.GetUtcNow();

        var actor =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        var category =
            new Category
            {
                Name =
                    new LocalizedText(
                        nameEn ,
                        nameAr) ,
                CreatedOn = now ,
                CreatedBy = actor ,
                IsDeleted = false
            };

        _dbContext.Categories.Add(category);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var response =
            await GetResponseByIdAsync(
                category.Id ,
                cancellationToken);

        if ( response is null )
        {
            throw new InvalidOperationException(
                "The category was created but could not be reloaded.");
        }

        return CategoryManagementResult<CategoryResponse>
            .Success(response);
    }

    public async Task<CategoryManagementResult<CategoryResponse>>
        UpdateAsync(
            int actorUserId ,
            int categoryId ,
            UpdateCategoryRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( actorUserId <= 0 ||
            categoryId <= 0 )
        {
            return CategoryManagementResult<CategoryResponse>
                .Failure(
                    CategoryManagementErrorCodes.InvalidRequest ,
                    "The category update request is invalid.");
        }

        if ( !TryNormalizeNames(
                request.NameEn ,
                request.NameAr ,
                out var nameEn ,
                out var nameAr ,
                out var validationError) )
        {
            return CategoryManagementResult<CategoryResponse>
                .Failure(
                    CategoryManagementErrorCodes.InvalidRequest ,
                    validationError);
        }

        var category =
            await _dbContext.Categories
                .AsTracking()
                .SingleOrDefaultAsync(
                    existingCategory =>
                        existingCategory.Id ==
                        categoryId ,
                    cancellationToken);

        if ( category is null ||
            category.IsDeleted )
        {
            return CategoryManagementResult<CategoryResponse>
                .Failure(
                    CategoryManagementErrorCodes.NotFound ,
                    "The active category was not found.");
        }

        if ( await HasDuplicateNameAsync(
                nameEn ,
                nameAr ,
                category.Id ,
                cancellationToken) )
        {
            return CategoryManagementResult<CategoryResponse>
                .Failure(
                    CategoryManagementErrorCodes.DuplicateName ,
                    "A category with the same Arabic or English name already exists.");
        }

        category.Name.Update(
            nameEn ,
            nameAr);

        category.LastModifiedOn =
            _timeProvider.GetUtcNow();

        category.LastModifiedBy =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var response =
            await GetResponseByIdAsync(
                category.Id ,
                cancellationToken);

        if ( response is null )
        {
            throw new InvalidOperationException(
                "The category was updated but could not be reloaded.");
        }

        return CategoryManagementResult<CategoryResponse>
            .Success(response);
    }

    public async Task<CategoryManagementOperationResult>
        DeleteAsync(
            int actorUserId ,
            int categoryId ,
            CancellationToken cancellationToken = default )
    {
        if ( actorUserId <= 0 ||
            categoryId <= 0 )
        {
            return CategoryManagementOperationResult.Failure(
                CategoryManagementErrorCodes.InvalidRequest ,
                "The category deletion request is invalid.");
        }

        var category =
            await _dbContext.Categories
                .AsTracking()
                .SingleOrDefaultAsync(
                    existingCategory =>
                        existingCategory.Id ==
                        categoryId ,
                    cancellationToken);

        if ( category is null ||
            category.IsDeleted )
        {
            return CategoryManagementOperationResult.Failure(
                CategoryManagementErrorCodes.NotFound ,
                "The active category was not found.");
        }

        var now =
            _timeProvider.GetUtcNow();

        var actor =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        category.IsDeleted = true;
        category.DeletedOn = now;
        category.DeletedBy = actor;
        category.LastModifiedOn = now;
        category.LastModifiedBy = actor;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return CategoryManagementOperationResult.Success();
    }

    public async Task<CategoryManagementOperationResult>
        RestoreAsync(
            int actorUserId ,
            int categoryId ,
            CancellationToken cancellationToken = default )
    {
        if ( actorUserId <= 0 ||
            categoryId <= 0 )
        {
            return CategoryManagementOperationResult.Failure(
                CategoryManagementErrorCodes.InvalidRequest ,
                "The category restoration request is invalid.");
        }

        var category =
            await _dbContext.Categories
                .AsTracking()
                .SingleOrDefaultAsync(
                    existingCategory =>
                        existingCategory.Id ==
                        categoryId ,
                    cancellationToken);

        if ( category is null )
        {
            return CategoryManagementOperationResult.Failure(
                CategoryManagementErrorCodes.NotFound ,
                "The category was not found.");
        }

        if ( !category.IsDeleted )
        {
            return CategoryManagementOperationResult.Failure(
                CategoryManagementErrorCodes.InvalidRequest ,
                "The category is already active.");
        }

        if ( await HasDuplicateNameAsync(
                category.Name.English ,
                category.Name.Arabic ,
                category.Id ,
                cancellationToken) )
        {
            return CategoryManagementOperationResult.Failure(
                CategoryManagementErrorCodes.DuplicateName ,
                "The category cannot be restored because another category uses the same name.");
        }

        var now =
            _timeProvider.GetUtcNow();

        category.IsDeleted = false;
        category.DeletedOn = null;
        category.DeletedBy = null;
        category.LastModifiedOn = now;
        category.LastModifiedBy =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return CategoryManagementOperationResult.Success();
    }
}