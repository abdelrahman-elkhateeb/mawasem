using Mawasem.Application.Features.Seasons.Contracts.Requests;
using Mawasem.Application.Features.Seasons.Contracts.Responses;
using Mawasem.Application.Features.Seasons.Models;

namespace Mawasem.Application.Features.Seasons.Interfaces;

public interface ISeasonManagementService
{
    Task<SeasonManagementResult<SeasonListResponse>> GetListAsync(
        GetSeasonsRequest request ,
        CancellationToken cancellationToken = default );

    Task<SeasonManagementResult<SeasonResponse>> GetByIdAsync(
        int seasonId ,
        CancellationToken cancellationToken = default );

    Task<SeasonManagementResult<SeasonResponse>> CreateAsync(
        int actorUserId ,
        CreateSeasonRequest request ,
        CancellationToken cancellationToken = default );

    Task<SeasonManagementResult<SeasonResponse>> UpdateAsync(
        int actorUserId ,
        int seasonId ,
        UpdateSeasonRequest request ,
        CancellationToken cancellationToken = default );

    Task<SeasonManagementOperationResult> DeleteAsync(
        int actorUserId ,
        int seasonId ,
        CancellationToken cancellationToken = default );

    Task<SeasonManagementOperationResult> RestoreAsync(
        int actorUserId ,
        int seasonId ,
        CancellationToken cancellationToken = default );
}