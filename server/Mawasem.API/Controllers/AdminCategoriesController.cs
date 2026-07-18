using Mawasem.API.Authorization;
using Mawasem.API.Extensions;
using Mawasem.Application.Features.Categories.Contracts.Requests;
using Mawasem.Application.Features.Categories.Interfaces;
using Mawasem.Application.Features.Categories.Models;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/admin/categories")]
public sealed class AdminCategoriesController : ControllerBase
{
    private readonly ICategoryManagementService
        _categoryManagementService;

    public AdminCategoriesController(
        ICategoryManagementService categoryManagementService )
    {
        _categoryManagementService =
            categoryManagementService;
    }

    [RequirePermission(
        SystemPermissions.Categories.View)]
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] GetCategoriesRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _categoryManagementService
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
        SystemPermissions.Categories.View)]
    [HttpGet("{categoryId:int}")]
    public async Task<IActionResult> GetById(
        int categoryId ,
        CancellationToken cancellationToken )
    {
        var result =
            await _categoryManagementService
                .GetByIdAsync(
                    categoryId ,
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
        SystemPermissions.Categories.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _categoryManagementService
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
                categoryId =
                    result.Response.Id
            } ,
            result.Response);
    }

    [RequirePermission(
        SystemPermissions.Categories.Edit)]
    [HttpPut("{categoryId:int}")]
    public async Task<IActionResult> Update(
        int categoryId ,
        [FromBody] UpdateCategoryRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _categoryManagementService
                .UpdateAsync(
                    actorUserId ,
                    categoryId ,
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
        SystemPermissions.Categories.Delete)]
    [HttpDelete("{categoryId:int}")]
    public async Task<IActionResult> Delete(
        int categoryId ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _categoryManagementService
                .DeleteAsync(
                    actorUserId ,
                    categoryId ,
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
        SystemPermissions.Categories.Restore)]
    [HttpPost("{categoryId:int}/restore")]
    public async Task<IActionResult> Restore(
        int categoryId ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _categoryManagementService
                .RestoreAsync(
                    actorUserId ,
                    categoryId ,
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
                CategoryManagementErrorCodes.NotFound =>
                    StatusCodes.Status404NotFound,

                CategoryManagementErrorCodes.DuplicateName =>
                    StatusCodes.Status409Conflict,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,
                Title =
                    "Category management request failed." ,
                Detail =
                    errorMessage
                    ?? "The category management request could not be completed."
            };

        problemDetails.Extensions["code"] =
            errorCode
            ?? CategoryManagementErrorCodes.InvalidRequest;

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
                    "Category management authentication failed." ,
                Detail =
                    "The authenticated dashboard account is invalid."
            };

        problemDetails.Extensions["code"] =
            CategoryManagementErrorCodes.InvalidRequest;

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
                    "Category management response failed." ,
                Detail =
                    "The category operation succeeded, but its response could not be returned."
            };

        problemDetails.Extensions["code"] =
            "categories.response_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}
