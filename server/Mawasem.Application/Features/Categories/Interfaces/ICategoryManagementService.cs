using Mawasem.Application.Features.Categories.Contracts.Requests;
using Mawasem.Application.Features.Categories.Contracts.Responses;
using Mawasem.Application.Features.Categories.Models;

namespace Mawasem.Application.Features.Categories.Interfaces;

public interface ICategoryManagementService
{
    Task<CategoryManagementResult<CategoryListResponse>> GetListAsync(
        GetCategoriesRequest request ,
        CancellationToken cancellationToken = default );

    Task<CategoryManagementResult<CategoryResponse>> GetByIdAsync(
        int categoryId ,
        CancellationToken cancellationToken = default );

    Task<CategoryManagementResult<CategoryResponse>> CreateAsync(
        int actorUserId ,
        CreateCategoryRequest request ,
        CancellationToken cancellationToken = default );

    Task<CategoryManagementResult<CategoryResponse>> UpdateAsync(
        int actorUserId ,
        int categoryId ,
        UpdateCategoryRequest request ,
        CancellationToken cancellationToken = default );

    Task<CategoryManagementOperationResult> DeleteAsync(
        int actorUserId ,
        int categoryId ,
        CancellationToken cancellationToken = default );

    Task<CategoryManagementOperationResult> RestoreAsync(
        int actorUserId ,
        int categoryId ,
        CancellationToken cancellationToken = default );
}