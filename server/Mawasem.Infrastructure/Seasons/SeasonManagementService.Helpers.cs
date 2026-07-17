using Mawasem.Application.Features.Seasons.Contracts.Responses;
using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Seasons;

public sealed partial class SeasonManagementService
{
    private static bool TryNormalizeValues(
        string? nameEnValue ,
        string? nameArValue ,
        string? descriptionEnValue ,
        string? descriptionArValue ,
        out string nameEn ,
        out string nameAr ,
        out string descriptionEn ,
        out string descriptionAr ,
        out string error )
    {
        nameEn =
            nameEnValue?.Trim()
            ?? string.Empty;

        nameAr =
            nameArValue?.Trim()
            ?? string.Empty;

        descriptionEn =
            descriptionEnValue?.Trim()
            ?? string.Empty;

        descriptionAr =
            descriptionArValue?.Trim()
            ?? string.Empty;

        if ( string.IsNullOrWhiteSpace(nameEn) )
        {
            error =
                "The English season name is required.";

            return false;
        }

        if ( string.IsNullOrWhiteSpace(nameAr) )
        {
            error =
                "The Arabic season name is required.";

            return false;
        }

        if ( nameEn.Length > MaximumNameLength )
        {
            error =
                $"The English season name cannot exceed {MaximumNameLength} characters.";

            return false;
        }

        if ( nameAr.Length > MaximumNameLength )
        {
            error =
                $"The Arabic season name cannot exceed {MaximumNameLength} characters.";

            return false;
        }

        if ( descriptionEn.Length >
            MaximumDescriptionLength )
        {
            error =
                $"The English season description cannot exceed {MaximumDescriptionLength} characters.";

            return false;
        }

        if ( descriptionAr.Length >
            MaximumDescriptionLength )
        {
            error =
                $"The Arabic season description cannot exceed {MaximumDescriptionLength} characters.";

            return false;
        }

        error = string.Empty;

        return true;
    }

    private async Task<bool> HasDuplicateNameAsync(
        string nameEn ,
        string nameAr ,
        int? excludedSeasonId ,
        CancellationToken cancellationToken )
    {
        return await _dbContext.Seasons
            .AsNoTracking()
            .AnyAsync(
                season =>
                    ( !excludedSeasonId.HasValue ||
                      season.Id !=
                      excludedSeasonId.Value ) &&
                    ( season.Name.English == nameEn ||
                      season.Name.Arabic == nameAr ) ,
                cancellationToken);
    }

    private async Task<SeasonResponse?> GetResponseByIdAsync(
        int seasonId ,
        CancellationToken cancellationToken )
    {
        return await ProjectSeasons(
                _dbContext.Seasons
                    .AsNoTracking()
                    .Where(season =>
                        season.Id == seasonId))
            .SingleOrDefaultAsync(
                cancellationToken);
    }

    private static IQueryable<SeasonResponse> ProjectSeasons(
        IQueryable<Season> query )
    {
        return query.Select(season =>
            new SeasonResponse
            {
                Id =
                    season.Id ,
                NameAr =
                    season.Name.Arabic ,
                NameEn =
                    season.Name.English ,
                DescriptionAr =
                    season.Description.Arabic ,
                DescriptionEn =
                    season.Description.English ,
                IsActive =
                    season.IsActive ,
                ProductCount =
                    season.Products.Count ,
                IsDeleted =
                    season.IsDeleted ,
                CreatedOn =
                    season.CreatedOn ,
                CreatedBy =
                    season.CreatedBy ,
                LastModifiedOn =
                    season.LastModifiedOn ,
                LastModifiedBy =
                    season.LastModifiedBy ,
                DeletedOn =
                    season.DeletedOn ,
                DeletedBy =
                    season.DeletedBy
            });
    }
}