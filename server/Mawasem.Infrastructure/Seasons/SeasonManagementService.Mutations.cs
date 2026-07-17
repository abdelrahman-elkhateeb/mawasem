using Mawasem.Application.Features.Seasons.Contracts.Requests;
using Mawasem.Application.Features.Seasons.Contracts.Responses;
using Mawasem.Application.Features.Seasons.Models;
using Mawasem.Domain.Catalog;
using Mawasem.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Mawasem.Infrastructure.Seasons;

public sealed partial class SeasonManagementService
{
    public async Task<SeasonManagementResult<SeasonResponse>>
        CreateAsync(
            int actorUserId ,
            CreateSeasonRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( actorUserId <= 0 )
        {
            return SeasonManagementResult<SeasonResponse>
                .Failure(
                    SeasonManagementErrorCodes.InvalidRequest ,
                    "The authenticated dashboard account is invalid.");
        }

        if ( !TryNormalizeValues(
                request.NameEn ,
                request.NameAr ,
                request.DescriptionEn ,
                request.DescriptionAr ,
                out var nameEn ,
                out var nameAr ,
                out var descriptionEn ,
                out var descriptionAr ,
                out var validationError) )
        {
            return SeasonManagementResult<SeasonResponse>
                .Failure(
                    SeasonManagementErrorCodes.InvalidRequest ,
                    validationError);
        }

        if ( await HasDuplicateNameAsync(
                nameEn ,
                nameAr ,
                excludedSeasonId: null ,
                cancellationToken) )
        {
            return SeasonManagementResult<SeasonResponse>
                .Failure(
                    SeasonManagementErrorCodes.DuplicateName ,
                    "A season with the same Arabic or English name already exists.");
        }

        var now =
            _timeProvider.GetUtcNow();

        var actor =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        var season =
            new Season
            {
                Name =
                    new LocalizedText(
                        nameEn ,
                        nameAr) ,
                Description =
                    new LocalizedText(
                        descriptionEn ,
                        descriptionAr) ,
                IsActive =
                    request.IsActive ,
                CreatedOn =
                    now ,
                CreatedBy =
                    actor ,
                IsDeleted =
                    false
            };

        _dbContext.Seasons.Add(season);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var response =
            await GetResponseByIdAsync(
                season.Id ,
                cancellationToken);

        if ( response is null )
        {
            throw new InvalidOperationException(
                "The season was created but could not be reloaded.");
        }

        return SeasonManagementResult<SeasonResponse>
            .Success(response);
    }

    public async Task<SeasonManagementResult<SeasonResponse>>
        UpdateAsync(
            int actorUserId ,
            int seasonId ,
            UpdateSeasonRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( actorUserId <= 0 ||
            seasonId <= 0 )
        {
            return SeasonManagementResult<SeasonResponse>
                .Failure(
                    SeasonManagementErrorCodes.InvalidRequest ,
                    "The season update request is invalid.");
        }

        if ( !TryNormalizeValues(
                request.NameEn ,
                request.NameAr ,
                request.DescriptionEn ,
                request.DescriptionAr ,
                out var nameEn ,
                out var nameAr ,
                out var descriptionEn ,
                out var descriptionAr ,
                out var validationError) )
        {
            return SeasonManagementResult<SeasonResponse>
                .Failure(
                    SeasonManagementErrorCodes.InvalidRequest ,
                    validationError);
        }

        var season =
            await _dbContext.Seasons
                .AsTracking()
                .SingleOrDefaultAsync(
                    existingSeason =>
                        existingSeason.Id ==
                        seasonId ,
                    cancellationToken);

        if ( season is null ||
            season.IsDeleted )
        {
            return SeasonManagementResult<SeasonResponse>
                .Failure(
                    SeasonManagementErrorCodes.NotFound ,
                    "The active season was not found.");
        }

        if ( await HasDuplicateNameAsync(
                nameEn ,
                nameAr ,
                season.Id ,
                cancellationToken) )
        {
            return SeasonManagementResult<SeasonResponse>
                .Failure(
                    SeasonManagementErrorCodes.DuplicateName ,
                    "A season with the same Arabic or English name already exists.");
        }

        season.Name.Update(
            nameEn ,
            nameAr);

        season.Description.Update(
            descriptionEn ,
            descriptionAr);

        season.IsActive =
            request.IsActive;

        season.LastModifiedOn =
            _timeProvider.GetUtcNow();

        season.LastModifiedBy =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var response =
            await GetResponseByIdAsync(
                season.Id ,
                cancellationToken);

        if ( response is null )
        {
            throw new InvalidOperationException(
                "The season was updated but could not be reloaded.");
        }

        return SeasonManagementResult<SeasonResponse>
            .Success(response);
    }

    public async Task<SeasonManagementOperationResult>
        DeleteAsync(
            int actorUserId ,
            int seasonId ,
            CancellationToken cancellationToken = default )
    {
        if ( actorUserId <= 0 ||
            seasonId <= 0 )
        {
            return SeasonManagementOperationResult.Failure(
                SeasonManagementErrorCodes.InvalidRequest ,
                "The season deletion request is invalid.");
        }

        var season =
            await _dbContext.Seasons
                .AsTracking()
                .SingleOrDefaultAsync(
                    existingSeason =>
                        existingSeason.Id ==
                        seasonId ,
                    cancellationToken);

        if ( season is null ||
            season.IsDeleted )
        {
            return SeasonManagementOperationResult.Failure(
                SeasonManagementErrorCodes.NotFound ,
                "The active season was not found.");
        }

        var now =
            _timeProvider.GetUtcNow();

        var actor =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        season.IsDeleted = true;
        season.DeletedOn = now;
        season.DeletedBy = actor;
        season.LastModifiedOn = now;
        season.LastModifiedBy = actor;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return SeasonManagementOperationResult.Success();
    }

    public async Task<SeasonManagementOperationResult>
        RestoreAsync(
            int actorUserId ,
            int seasonId ,
            CancellationToken cancellationToken = default )
    {
        if ( actorUserId <= 0 ||
            seasonId <= 0 )
        {
            return SeasonManagementOperationResult.Failure(
                SeasonManagementErrorCodes.InvalidRequest ,
                "The season restoration request is invalid.");
        }

        var season =
            await _dbContext.Seasons
                .AsTracking()
                .SingleOrDefaultAsync(
                    existingSeason =>
                        existingSeason.Id ==
                        seasonId ,
                    cancellationToken);

        if ( season is null )
        {
            return SeasonManagementOperationResult.Failure(
                SeasonManagementErrorCodes.NotFound ,
                "The season was not found.");
        }

        if ( !season.IsDeleted )
        {
            return SeasonManagementOperationResult.Failure(
                SeasonManagementErrorCodes.InvalidRequest ,
                "The season is already active.");
        }

        if ( await HasDuplicateNameAsync(
                season.Name.English ,
                season.Name.Arabic ,
                season.Id ,
                cancellationToken) )
        {
            return SeasonManagementOperationResult.Failure(
                SeasonManagementErrorCodes.DuplicateName ,
                "The season cannot be restored because another season uses the same name.");
        }

        var now =
            _timeProvider.GetUtcNow();

        season.IsDeleted = false;
        season.DeletedOn = null;
        season.DeletedBy = null;
        season.LastModifiedOn = now;

        season.LastModifiedBy =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return SeasonManagementOperationResult.Success();
    }
}