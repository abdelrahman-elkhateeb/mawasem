using Mawasem.API.Authorization;
using Mawasem.API.Extensions;
using Mawasem.Application.Features.Seasons.Contracts.Requests;
using Mawasem.Application.Features.Seasons.Interfaces;
using Mawasem.Application.Features.Seasons.Models;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/admin/seasons")]
public sealed class AdminSeasonsController : ControllerBase
{
    private readonly ISeasonManagementService
        _seasonManagementService;

    public AdminSeasonsController(
        ISeasonManagementService seasonManagementService )
    {
        _seasonManagementService =
            seasonManagementService;
    }

    [RequirePermission(
        SystemPermissions.Seasons.View)]
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] GetSeasonsRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _seasonManagementService
                .GetListAsync(
                    request ,
                    cancellationToken);

        if ( !result.Succeeded )
        {
            return CreateFailureResponse(
                result.ErrorCode ,
                result.ErrorMessage);
        }

        if ( result.Response is null )
        {
            return CreateUnexpectedFailureResponse();
        }

        return Ok(result.Response);
    }

    [RequirePermission(
        SystemPermissions.Seasons.View)]
    [HttpGet("{seasonId:int}")]
    public async Task<IActionResult> GetById(
        int seasonId ,
        CancellationToken cancellationToken )
    {
        var result =
            await _seasonManagementService
                .GetByIdAsync(
                    seasonId ,
                    cancellationToken);

        if ( !result.Succeeded )
        {
            return CreateFailureResponse(
                result.ErrorCode ,
                result.ErrorMessage);
        }

        if ( result.Response is null )
        {
            return CreateUnexpectedFailureResponse();
        }

        return Ok(result.Response);
    }

    [RequirePermission(
        SystemPermissions.Seasons.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSeasonRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _seasonManagementService
                .CreateAsync(
                    actorUserId ,
                    request ,
                    cancellationToken);

        if ( !result.Succeeded )
        {
            return CreateFailureResponse(
                result.ErrorCode ,
                result.ErrorMessage);
        }

        if ( result.Response is null )
        {
            return CreateUnexpectedFailureResponse();
        }

        return CreatedAtAction(
            nameof(GetById) ,
            new
            {
                seasonId =
                    result.Response.Id
            } ,
            result.Response);
    }

    [RequirePermission(
        SystemPermissions.Seasons.Edit)]
    [HttpPut("{seasonId:int}")]
    public async Task<IActionResult> Update(
        int seasonId ,
        [FromBody] UpdateSeasonRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _seasonManagementService
                .UpdateAsync(
                    actorUserId ,
                    seasonId ,
                    request ,
                    cancellationToken);

        if ( !result.Succeeded )
        {
            return CreateFailureResponse(
                result.ErrorCode ,
                result.ErrorMessage);
        }

        if ( result.Response is null )
        {
            return CreateUnexpectedFailureResponse();
        }

        return Ok(result.Response);
    }

    [RequirePermission(
        SystemPermissions.Seasons.Delete)]
    [HttpDelete("{seasonId:int}")]
    public async Task<IActionResult> Delete(
        int seasonId ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _seasonManagementService
                .DeleteAsync(
                    actorUserId ,
                    seasonId ,
                    cancellationToken);

        if ( !result.Succeeded )
        {
            return CreateFailureResponse(
                result.ErrorCode ,
                result.ErrorMessage);
        }

        return NoContent();
    }

    [RequirePermission(
        SystemPermissions.Seasons.Restore)]
    [HttpPost("{seasonId:int}/restore")]
    public async Task<IActionResult> Restore(
        int seasonId ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _seasonManagementService
                .RestoreAsync(
                    actorUserId ,
                    seasonId ,
                    cancellationToken);

        if ( !result.Succeeded )
        {
            return CreateFailureResponse(
                result.ErrorCode ,
                result.ErrorMessage);
        }

        return NoContent();
    }

    private bool TryGetActorUserId(
        out int actorUserId )
    {
        return User.TryGetUserId(
            out actorUserId);
    }

    private IActionResult CreateFailureResponse(
        string? errorCode ,
        string? errorMessage )
    {
        var statusCode =
            errorCode switch
            {
                SeasonManagementErrorCodes.NotFound =>
                    StatusCodes.Status404NotFound,

                SeasonManagementErrorCodes.DuplicateName =>
                    StatusCodes.Status409Conflict,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,
                Title =
                    "Season management request failed." ,
                Detail =
                    errorMessage
                    ?? "The season management request could not be completed."
            };

        problemDetails.Extensions["code"] =
            errorCode
            ?? SeasonManagementErrorCodes.InvalidRequest;

        return StatusCode(
            statusCode ,
            problemDetails);
    }

    private IActionResult CreateInvalidActorResponse()
    {
        var problemDetails =
            new ProblemDetails
            {
                Status =
                    StatusCodes.Status401Unauthorized ,
                Title =
                    "Season management authentication failed." ,
                Detail =
                    "The authenticated dashboard account is invalid."
            };

        problemDetails.Extensions["code"] =
            SeasonManagementErrorCodes.InvalidRequest;

        return Unauthorized(problemDetails);
    }

    private IActionResult CreateUnexpectedFailureResponse()
    {
        var problemDetails =
            new ProblemDetails
            {
                Status =
                    StatusCodes.Status500InternalServerError ,
                Title =
                    "Season management response failed." ,
                Detail =
                    "The season operation succeeded, but its response could not be returned."
            };

        problemDetails.Extensions["code"] =
            "seasons.response_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}
