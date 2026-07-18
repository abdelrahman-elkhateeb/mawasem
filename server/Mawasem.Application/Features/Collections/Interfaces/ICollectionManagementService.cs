using Mawasem.Application.Features.Collections.Contracts.Requests;
using Mawasem.Application.Features.Collections.Contracts.Responses;
using Mawasem.Application.Features.Collections.Models;

namespace Mawasem.Application.Features.Collections.Interfaces;

public interface ICollectionManagementService
{
    Task<CollectionManagementResult<CollectionListResponse>> GetListAsync(
        GetCollectionsRequest request ,
        CancellationToken cancellationToken = default );

    Task<CollectionManagementResult<CollectionResponse>> GetByIdAsync(
        int collectionId ,
        CancellationToken cancellationToken = default );

    Task<CollectionManagementResult<CollectionResponse>> CreateAsync(
        int actorUserId ,
        CreateCollectionRequest request ,
        CancellationToken cancellationToken = default );

    Task<CollectionManagementResult<CollectionResponse>> UpdateAsync(
        int actorUserId ,
        int collectionId ,
        UpdateCollectionRequest request ,
        CancellationToken cancellationToken = default );

    Task<CollectionManagementOperationResult> DeleteAsync(
        int actorUserId ,
        int collectionId ,
        CancellationToken cancellationToken = default );

    Task<CollectionManagementOperationResult> RestoreAsync(
        int actorUserId ,
        int collectionId ,
        CancellationToken cancellationToken = default );
}