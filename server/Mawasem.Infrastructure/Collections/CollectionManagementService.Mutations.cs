using Mawasem.Application.Features.Collections.Contracts.Requests;
using Mawasem.Application.Features.Collections.Contracts.Responses;
using Mawasem.Application.Features.Collections.Models;
using Mawasem.Domain.Catalog;
using Mawasem.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Mawasem.Infrastructure.Collections;

public sealed partial class CollectionManagementService
{
    public async Task<CollectionManagementResult<CollectionResponse>>
        CreateAsync(
            int actorUserId ,
            CreateCollectionRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( actorUserId <= 0 )
        {
            return CollectionManagementResult<CollectionResponse>
                .Failure(
                    CollectionManagementErrorCodes.InvalidRequest ,
                    "The authenticated dashboard account is invalid.");
        }

        if ( !TryNormalizeNames(
                request.NameEn ,
                request.NameAr ,
                out var nameEn ,
                out var nameAr ,
                out var validationError) )
        {
            return CollectionManagementResult<CollectionResponse>
                .Failure(
                    CollectionManagementErrorCodes.InvalidRequest ,
                    validationError);
        }

        if ( await HasDuplicateNameAsync(
                nameEn ,
                nameAr ,
                excludedCollectionId: null ,
                cancellationToken) )
        {
            return CollectionManagementResult<CollectionResponse>
                .Failure(
                    CollectionManagementErrorCodes.DuplicateName ,
                    "A collection with the same Arabic or English name already exists.");
        }

        var now =
            _timeProvider.GetUtcNow();

        var actor =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        var collection =
            new Collection
            {
                Name =
                    new LocalizedText(
                        nameEn ,
                        nameAr) ,
                CreatedOn = now ,
                CreatedBy = actor ,
                IsDeleted = false
            };

        _dbContext.Collections.Add(collection);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var response =
            await GetResponseByIdAsync(
                collection.Id ,
                cancellationToken);

        if ( response is null )
        {
            throw new InvalidOperationException(
                "The collection was created but could not be reloaded.");
        }

        return CollectionManagementResult<CollectionResponse>
            .Success(response);
    }

    public async Task<CollectionManagementResult<CollectionResponse>>
        UpdateAsync(
            int actorUserId ,
            int collectionId ,
            UpdateCollectionRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( actorUserId <= 0 ||
            collectionId <= 0 )
        {
            return CollectionManagementResult<CollectionResponse>
                .Failure(
                    CollectionManagementErrorCodes.InvalidRequest ,
                    "The collection update request is invalid.");
        }

        if ( !TryNormalizeNames(
                request.NameEn ,
                request.NameAr ,
                out var nameEn ,
                out var nameAr ,
                out var validationError) )
        {
            return CollectionManagementResult<CollectionResponse>
                .Failure(
                    CollectionManagementErrorCodes.InvalidRequest ,
                    validationError);
        }

        var collection =
            await _dbContext.Collections
                .AsTracking()
                .SingleOrDefaultAsync(
                    existingCollection =>
                        existingCollection.Id ==
                        collectionId ,
                    cancellationToken);

        if ( collection is null ||
            collection.IsDeleted )
        {
            return CollectionManagementResult<CollectionResponse>
                .Failure(
                    CollectionManagementErrorCodes.NotFound ,
                    "The active collection was not found.");
        }

        if ( await HasDuplicateNameAsync(
                nameEn ,
                nameAr ,
                collection.Id ,
                cancellationToken) )
        {
            return CollectionManagementResult<CollectionResponse>
                .Failure(
                    CollectionManagementErrorCodes.DuplicateName ,
                    "A collection with the same Arabic or English name already exists.");
        }

        collection.Name.Update(
            nameEn ,
            nameAr);

        collection.LastModifiedOn =
            _timeProvider.GetUtcNow();

        collection.LastModifiedBy =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var response =
            await GetResponseByIdAsync(
                collection.Id ,
                cancellationToken);

        if ( response is null )
        {
            throw new InvalidOperationException(
                "The collection was updated but could not be reloaded.");
        }

        return CollectionManagementResult<CollectionResponse>
            .Success(response);
    }

    public async Task<CollectionManagementOperationResult>
        DeleteAsync(
            int actorUserId ,
            int collectionId ,
            CancellationToken cancellationToken = default )
    {
        if ( actorUserId <= 0 ||
            collectionId <= 0 )
        {
            return CollectionManagementOperationResult.Failure(
                CollectionManagementErrorCodes.InvalidRequest ,
                "The collection deletion request is invalid.");
        }

        var collection =
            await _dbContext.Collections
                .AsTracking()
                .SingleOrDefaultAsync(
                    existingCollection =>
                        existingCollection.Id ==
                        collectionId ,
                    cancellationToken);

        if ( collection is null ||
            collection.IsDeleted )
        {
            return CollectionManagementOperationResult.Failure(
                CollectionManagementErrorCodes.NotFound ,
                "The active collection was not found.");
        }

        var now =
            _timeProvider.GetUtcNow();

        var actor =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        collection.IsDeleted = true;
        collection.DeletedOn = now;
        collection.DeletedBy = actor;
        collection.LastModifiedOn = now;
        collection.LastModifiedBy = actor;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return CollectionManagementOperationResult.Success();
    }

    public async Task<CollectionManagementOperationResult>
        RestoreAsync(
            int actorUserId ,
            int collectionId ,
            CancellationToken cancellationToken = default )
    {
        if ( actorUserId <= 0 ||
            collectionId <= 0 )
        {
            return CollectionManagementOperationResult.Failure(
                CollectionManagementErrorCodes.InvalidRequest ,
                "The collection restoration request is invalid.");
        }

        var collection =
            await _dbContext.Collections
                .AsTracking()
                .SingleOrDefaultAsync(
                    existingCollection =>
                        existingCollection.Id ==
                        collectionId ,
                    cancellationToken);

        if ( collection is null )
        {
            return CollectionManagementOperationResult.Failure(
                CollectionManagementErrorCodes.NotFound ,
                "The collection was not found.");
        }

        if ( !collection.IsDeleted )
        {
            return CollectionManagementOperationResult.Failure(
                CollectionManagementErrorCodes.InvalidRequest ,
                "The collection is already active.");
        }

        if ( await HasDuplicateNameAsync(
                collection.Name.English ,
                collection.Name.Arabic ,
                collection.Id ,
                cancellationToken) )
        {
            return CollectionManagementOperationResult.Failure(
                CollectionManagementErrorCodes.DuplicateName ,
                "The collection cannot be restored because another collection uses the same name.");
        }

        var now =
            _timeProvider.GetUtcNow();

        collection.IsDeleted = false;
        collection.DeletedOn = null;
        collection.DeletedBy = null;
        collection.LastModifiedOn = now;
        collection.LastModifiedBy =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return CollectionManagementOperationResult.Success();
    }
}