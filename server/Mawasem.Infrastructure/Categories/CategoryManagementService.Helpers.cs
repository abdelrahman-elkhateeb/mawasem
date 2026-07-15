using Mawasem.Application.Features.Categories.Contracts.Responses;
using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Categories;

public sealed partial class CategoryManagementService
{
    private static bool TryNormalizeNames(
        string? nameEnValue ,
        string? nameArValue ,
        out string nameEn ,
        out string nameAr ,
        out string error )
    {
        nameEn =
            nameEnValue?.Trim()
            ?? string.Empty;

        nameAr =
            nameArValue?.Trim()
            ?? string.Empty;

        if ( string.IsNullOrWhiteSpace(nameEn) )
        {
            error =
                "The English category name is required.";

            return false;
        }

        if ( string.IsNullOrWhiteSpace(nameAr) )
        {
            error =
                "The Arabic category name is required.";

            return false;
        }

        if ( nameEn.Length > MaximumNameLength )
        {
            error =
                $"The English category name cannot exceed {MaximumNameLength} characters.";

            return false;
        }

        if ( nameAr.Length > MaximumNameLength )
        {
            error =
                $"The Arabic category name cannot exceed {MaximumNameLength} characters.";

            return false;
        }

        error = string.Empty;

        return true;
    }

    private async Task<bool> HasDuplicateNameAsync(
        string nameEn ,
        string nameAr ,
        int? excludedCategoryId ,
        CancellationToken cancellationToken )
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .AnyAsync(
                category =>
                    ( !excludedCategoryId.HasValue ||
                      category.Id !=
                      excludedCategoryId.Value ) &&
                    ( category.Name.English == nameEn ||
                      category.Name.Arabic == nameAr ) ,
                cancellationToken);
    }

    private async Task<CategoryResponse?> GetResponseByIdAsync(
        int categoryId ,
        CancellationToken cancellationToken )
    {
        return await ProjectCategories(
                _dbContext.Categories
                    .AsNoTracking()
                    .Where(category =>
                        category.Id ==
                        categoryId))
            .SingleOrDefaultAsync(
                cancellationToken);
    }

    private static IQueryable<CategoryResponse>
        ProjectCategories(
            IQueryable<Category> query )
    {
        return query.Select(category =>
            new CategoryResponse
            {
                Id = category.Id ,
                NameAr =
                    category.Name.Arabic ,
                NameEn =
                    category.Name.English ,
                ProductCount =
                    category.ProductCategories.Count ,
                IsDeleted =
                    category.IsDeleted ,
                CreatedOn =
                    category.CreatedOn ,
                CreatedBy =
                    category.CreatedBy ,
                LastModifiedOn =
                    category.LastModifiedOn ,
                LastModifiedBy =
                    category.LastModifiedBy ,
                DeletedOn =
                    category.DeletedOn ,
                DeletedBy =
                    category.DeletedBy
            });
    }
}