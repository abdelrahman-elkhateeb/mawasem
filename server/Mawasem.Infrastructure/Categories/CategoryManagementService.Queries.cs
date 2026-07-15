using Mawasem.Application.Features.Categories.Contracts.Requests;
using Mawasem.Application.Features.Categories.Contracts.Responses;
using Mawasem.Application.Features.Categories.Models;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Categories;

public sealed partial class CategoryManagementService
{
    public async Task<CategoryManagementResult<CategoryListResponse>>
        GetListAsync(
            GetCategoriesRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( request.PageNumber <= 0 )
        {
            return CategoryManagementResult<CategoryListResponse>
                .Failure(
                    CategoryManagementErrorCodes.InvalidRequest ,
                    "Page number must be greater than zero.");
        }

        if ( request.PageSize <= 0 ||
            request.PageSize > MaximumPageSize )
        {
            return CategoryManagementResult<CategoryListResponse>
                .Failure(
                    CategoryManagementErrorCodes.InvalidRequest ,
                    $"Page size must be between 1 and {MaximumPageSize}.");
        }

        var skipCount =
            (long)( request.PageNumber - 1 ) *
            request.PageSize;

        if ( skipCount > int.MaxValue )
        {
            return CategoryManagementResult<CategoryListResponse>
                .Failure(
                    CategoryManagementErrorCodes.InvalidRequest ,
                    "The requested page is outside the supported range.");
        }

        var search =
            request.Search?.Trim();

        if ( search?.Length > MaximumSearchLength )
        {
            return CategoryManagementResult<CategoryListResponse>
                .Failure(
                    CategoryManagementErrorCodes.InvalidRequest ,
                    $"Search text cannot exceed {MaximumSearchLength} characters.");
        }

        var categoryQuery =
            _dbContext.Categories
                .AsNoTracking();

        if ( !request.IncludeDeleted )
        {
            categoryQuery =
                categoryQuery.Where(category =>
                    !category.IsDeleted);
        }

        if ( !string.IsNullOrWhiteSpace(search) )
        {
            categoryQuery =
                categoryQuery.Where(category =>
                    category.Name.English.Contains(search) ||
                    category.Name.Arabic.Contains(search));
        }

        var totalCount =
            await categoryQuery.CountAsync(
                cancellationToken);

        var items =
            await ProjectCategories(categoryQuery)
                .OrderBy(category =>
                    category.NameEn)
                .ThenBy(category =>
                    category.Id)
                .Skip((int)skipCount)
                .Take(request.PageSize)
                .ToArrayAsync(cancellationToken);

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)request.PageSize);

        var response =
            new CategoryListResponse
            {
                Items = items ,
                PageNumber = request.PageNumber ,
                PageSize = request.PageSize ,
                TotalCount = totalCount ,
                TotalPages = totalPages
            };

        return CategoryManagementResult<CategoryListResponse>
            .Success(response);
    }

    public async Task<CategoryManagementResult<CategoryResponse>>
        GetByIdAsync(
            int categoryId ,
            CancellationToken cancellationToken = default )
    {
        if ( categoryId <= 0 )
        {
            return CategoryManagementResult<CategoryResponse>
                .Failure(
                    CategoryManagementErrorCodes.NotFound ,
                    "The category was not found.");
        }

        var response =
            await GetResponseByIdAsync(
                categoryId ,
                cancellationToken);

        if ( response is null )
        {
            return CategoryManagementResult<CategoryResponse>
                .Failure(
                    CategoryManagementErrorCodes.NotFound ,
                    "The category was not found.");
        }

        return CategoryManagementResult<CategoryResponse>
            .Success(response);
    }
}