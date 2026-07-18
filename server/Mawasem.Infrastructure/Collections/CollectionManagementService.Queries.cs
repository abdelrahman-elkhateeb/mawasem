using Mawasem.Application.Features.Collections.Contracts.Requests;
using Mawasem.Application.Features.Collections.Contracts.Responses;
using Mawasem.Application.Features.Collections.Models;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Collections;

public sealed partial class CollectionManagementService
{
    public async Task<CollectionManagementResult<CollectionListResponse>>
        GetListAsync(
            GetCollectionsRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( request.PageNumber <= 0 )
        {
            return CollectionManagementResult<CollectionListResponse>
                .Failure(
                    CollectionManagementErrorCodes.InvalidRequest ,
                    "Page number must be greater than zero.");
        }

        if ( request.PageSize <= 0 ||
            request.PageSize > MaximumPageSize )
        {
            return CollectionManagementResult<CollectionListResponse>
                .Failure(
                    CollectionManagementErrorCodes.InvalidRequest ,
                    $"Page size must be between 1 and {MaximumPageSize}.");
        }

        var skipCount =
            (long)( request.PageNumber - 1 ) *
            request.PageSize;

        if ( skipCount > int.MaxValue )
        {
            return CollectionManagementResult<CollectionListResponse>
                .Failure(
                    CollectionManagementErrorCodes.InvalidRequest ,
                    "The requested page is outside the supported range.");
        }

        var search =
            request.Search?.Trim();

        if ( search?.Length > MaximumSearchLength )
        {
            return CollectionManagementResult<CollectionListResponse>
                .Failure(
                    CollectionManagementErrorCodes.InvalidRequest ,
                    $"Search text cannot exceed {MaximumSearchLength} characters.");
        }

        var collectionQuery =
            _dbContext.Collections
                .AsNoTracking();

        if ( !request.IncludeDeleted )
        {
            collectionQuery =
                collectionQuery.Where(collection =>
                    !collection.IsDeleted);
        }

        if ( !string.IsNullOrWhiteSpace(search) )
        {
            collectionQuery =
                collectionQuery.Where(collection =>
                    collection.Name.English.Contains(search) ||
                    collection.Name.Arabic.Contains(search));
        }

        var totalCount =
            await collectionQuery.CountAsync(
                cancellationToken);

        var items =
            await ProjectCollections(collectionQuery)
                .OrderBy(collection =>
                    collection.NameEn)
                .ThenBy(collection =>
                    collection.Id)
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
            new CollectionListResponse
            {
                Items = items ,
                PageNumber = request.PageNumber ,
                PageSize = request.PageSize ,
                TotalCount = totalCount ,
                TotalPages = totalPages
            };

        return CollectionManagementResult<CollectionListResponse>
            .Success(response);
    }

    public async Task<CollectionManagementResult<CollectionResponse>>
        GetByIdAsync(
            int collectionId ,
            CancellationToken cancellationToken = default )
    {
        if ( collectionId <= 0 )
        {
            return CollectionManagementResult<CollectionResponse>
                .Failure(
                    CollectionManagementErrorCodes.NotFound ,
                    "The collection was not found.");
        }

        var response =
            await GetResponseByIdAsync(
                collectionId ,
                cancellationToken);

        if ( response is null )
        {
            return CollectionManagementResult<CollectionResponse>
                .Failure(
                    CollectionManagementErrorCodes.NotFound ,
                    "The collection was not found.");
        }

        return CollectionManagementResult<CollectionResponse>
            .Success(response);
    }
}