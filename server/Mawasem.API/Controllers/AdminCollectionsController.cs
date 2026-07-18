using Mawasem.API.Authorization;
using Mawasem.API.Extensions;
using Mawasem.Application.Features.Collections.Contracts.Requests;
using Mawasem.Application.Features.Collections.Interfaces;
using Mawasem.Application.Features.Collections.Models;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/admin/collections")]
public sealed class AdminCollectionsController : ControllerBase
{
    private readonly ICollectionManagementService
        _collectionManagementService;

    public AdminCollectionsController(
        ICollectionManagementService collectionManagementService )
    {
        _collectionManagementService =
            collectionManagementService;
    }

    [RequirePermission(
        SystemPermissions.Collections.View)]
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] GetCollectionsRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _collectionManagementService
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
        SystemPermissions.Collections.View)]
    [HttpGet("{collectionId:int}")]
    public async Task<IActionResult> GetById(
        int collectionId ,
        CancellationToken cancellationToken )
    {
        var result =
            await _collectionManagementService
                .GetByIdAsync(
                    collectionId ,
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
        SystemPermissions.Collections.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCollectionRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _collectionManagementService
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
                collectionId =
                    result.Response.Id
            } ,
            result.Response);
    }

    [RequirePermission(
        SystemPermissions.Collections.Edit)]
    [HttpPut("{collectionId:int}")]
    public async Task<IActionResult> Update(
        int collectionId ,
        [FromBody] UpdateCollectionRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _collectionManagementService
                .UpdateAsync(
                    actorUserId ,
                    collectionId ,
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
        SystemPermissions.Collections.Delete)]
    [HttpDelete("{collectionId:int}")]
    public async Task<IActionResult> Delete(
        int collectionId ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _collectionManagementService
                .DeleteAsync(
                    actorUserId ,
                    collectionId ,
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
        SystemPermissions.Collections.Restore)]
    [HttpPost("{collectionId:int}/restore")]
    public async Task<IActionResult> Restore(
        int collectionId ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _collectionManagementService
                .RestoreAsync(
                    actorUserId ,
                    collectionId ,
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
                CollectionManagementErrorCodes.NotFound =>
                    StatusCodes.Status404NotFound,

                CollectionManagementErrorCodes.DuplicateName =>
                    StatusCodes.Status409Conflict,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,
                Title =
                    "Collection management request failed." ,
                Detail =
                    errorMessage
                    ?? "The collection management request could not be completed."
            };

        problemDetails.Extensions["code"] =
            errorCode
            ?? CollectionManagementErrorCodes.InvalidRequest;

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
                    "Collection management authentication failed." ,
                Detail =
                    "The authenticated dashboard account is invalid."
            };

        problemDetails.Extensions["code"] =
            CollectionManagementErrorCodes.InvalidRequest;

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
                    "Collection management response failed." ,
                Detail =
                    "The collection operation succeeded, but its response could not be returned."
            };

        problemDetails.Extensions["code"] =
            "collections.response_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}