using Mawasem.Application.Features.Collections.Contracts.Responses;
using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Collections;

public sealed partial class CollectionManagementService
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
                "The English collection name is required.";

            return false;
        }

        if ( string.IsNullOrWhiteSpace(nameAr) )
        {
            error =
                "The Arabic collection name is required.";

            return false;
        }

        if ( nameEn.Length > MaximumNameLength )
        {
            error =
                $"The English collection name cannot exceed {MaximumNameLength} characters.";

            return false;
        }

        if ( nameAr.Length > MaximumNameLength )
        {
            error =
                $"The Arabic collection name cannot exceed {MaximumNameLength} characters.";

            return false;
        }

        error = string.Empty;

        return true;
    }

    private async Task<bool> HasDuplicateNameAsync(
        string nameEn ,
        string nameAr ,
        int? excludedCollectionId ,
        CancellationToken cancellationToken )
    {
        return await _dbContext.Collections
            .AsNoTracking()
            .AnyAsync(
                collection =>
                    ( !excludedCollectionId.HasValue ||
                      collection.Id !=
                      excludedCollectionId.Value ) &&
                    ( collection.Name.English == nameEn ||
                      collection.Name.Arabic == nameAr ) ,
                cancellationToken);
    }

    private async Task<bool> SeasonExistsAsync(
        int seasonId ,
        CancellationToken cancellationToken )
    {
        return await _dbContext.Seasons
            .AsNoTracking()
            .AnyAsync(
                season =>
                    season.Id == seasonId &&
                    !season.IsDeleted ,
                cancellationToken);
    }

    private async Task<CollectionResponse?> GetResponseByIdAsync(
        int collectionId ,
        CancellationToken cancellationToken )
    {
        return await ProjectCollections(
                _dbContext.Collections
                    .AsNoTracking()
                    .Where(collection =>
                        collection.Id ==
                        collectionId))
            .SingleOrDefaultAsync(
                cancellationToken);
    }

    private static IQueryable<CollectionResponse>
        ProjectCollections(
            IQueryable<Collection> query )
    {
        return query.Select(collection =>
            new CollectionResponse
            {
                Id = collection.Id ,
                NameAr =
                    collection.Name.Arabic ,
                NameEn =
                    collection.Name.English ,
                SeasonId =
                    collection.SeasonId ,
                SeasonNameAr =
                    collection.Season.Name.Arabic ,
                SeasonNameEn =
                    collection.Season.Name.English ,
                ProductCount =
                    collection.ProductCollections.Count ,
                IsDeleted =
                    collection.IsDeleted ,
                CreatedOn =
                    collection.CreatedOn ,
                CreatedBy =
                    collection.CreatedBy ,
                LastModifiedOn =
                    collection.LastModifiedOn ,
                LastModifiedBy =
                    collection.LastModifiedBy ,
                DeletedOn =
                    collection.DeletedOn ,
                DeletedBy =
                    collection.DeletedBy
            });
    }
}