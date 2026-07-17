using Mawasem.Application.Features.Seasons.Contracts.Requests;
using Mawasem.Application.Features.Seasons.Contracts.Responses;
using Mawasem.Application.Features.Seasons.Models;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Seasons;

public sealed partial class SeasonManagementService
{
    public async Task<SeasonManagementResult<SeasonListResponse>>
        GetListAsync(
            GetSeasonsRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( request.PageNumber <= 0 )
        {
            return SeasonManagementResult<SeasonListResponse>
                .Failure(
                    SeasonManagementErrorCodes.InvalidRequest ,
                    "Page number must be greater than zero.");
        }

        if ( request.PageSize <= 0 ||
            request.PageSize > MaximumPageSize )
        {
            return SeasonManagementResult<SeasonListResponse>
                .Failure(
                    SeasonManagementErrorCodes.InvalidRequest ,
                    $"Page size must be between 1 and {MaximumPageSize}.");
        }

        var skipCount =
            (long)( request.PageNumber - 1 ) *
            request.PageSize;

        if ( skipCount > int.MaxValue )
        {
            return SeasonManagementResult<SeasonListResponse>
                .Failure(
                    SeasonManagementErrorCodes.InvalidRequest ,
                    "The requested page is outside the supported range.");
        }

        var search =
            request.Search?.Trim();

        if ( search?.Length > MaximumSearchLength )
        {
            return SeasonManagementResult<SeasonListResponse>
                .Failure(
                    SeasonManagementErrorCodes.InvalidRequest ,
                    $"Search text cannot exceed {MaximumSearchLength} characters.");
        }

        var seasonQuery =
            _dbContext.Seasons
                .AsNoTracking();

        if ( !request.IncludeDeleted )
        {
            seasonQuery =
                seasonQuery.Where(season =>
                    !season.IsDeleted);
        }

        if ( request.IsActive.HasValue )
        {
            seasonQuery =
                seasonQuery.Where(season =>
                    season.IsActive ==
                    request.IsActive.Value);
        }

        if ( !string.IsNullOrWhiteSpace(search) )
        {
            seasonQuery =
                seasonQuery.Where(season =>
                    season.Name.English.Contains(search) ||
                    season.Name.Arabic.Contains(search) ||
                    season.Description.English.Contains(search) ||
                    season.Description.Arabic.Contains(search));
        }

        var totalCount =
            await seasonQuery.CountAsync(
                cancellationToken);

        var items =
            await ProjectSeasons(seasonQuery)
                .OrderBy(season =>
                    season.NameEn)
                .ThenBy(season =>
                    season.Id)
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
            new SeasonListResponse
            {
                Items =
                    items ,
                PageNumber =
                    request.PageNumber ,
                PageSize =
                    request.PageSize ,
                TotalCount =
                    totalCount ,
                TotalPages =
                    totalPages
            };

        return SeasonManagementResult<SeasonListResponse>
            .Success(response);
    }

    public async Task<SeasonManagementResult<SeasonResponse>>
        GetByIdAsync(
            int seasonId ,
            CancellationToken cancellationToken = default )
    {
        if ( seasonId <= 0 )
        {
            return SeasonManagementResult<SeasonResponse>
                .Failure(
                    SeasonManagementErrorCodes.NotFound ,
                    "The season was not found.");
        }

        var response =
            await GetResponseByIdAsync(
                seasonId ,
                cancellationToken);

        if ( response is null )
        {
            return SeasonManagementResult<SeasonResponse>
                .Failure(
                    SeasonManagementErrorCodes.NotFound ,
                    "The season was not found.");
        }

        return SeasonManagementResult<SeasonResponse>
            .Success(response);
    }
}