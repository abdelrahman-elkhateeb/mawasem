using Mawasem.API.Authorization;
using Mawasem.API.Extensions;
using Mawasem.Application.Features.Brands.Contracts.Requests;
using Mawasem.Application.Features.Brands.Interfaces;
using Mawasem.Application.Features.Brands.Models;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/admin/brands")]
public sealed class AdminBrandsController : ControllerBase
{
    private readonly IBrandManagementService
        _brandManagementService;

    public AdminBrandsController(
        IBrandManagementService brandManagementService )
    {
        _brandManagementService =
            brandManagementService;
    }

    [RequirePermission(
        SystemPermissions.Brands.View)]
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] GetBrandsRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _brandManagementService
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
        SystemPermissions.Brands.View)]
    [HttpGet("{brandId:int}")]
    public async Task<IActionResult> GetById(
        int brandId ,
        CancellationToken cancellationToken )
    {
        var result =
            await _brandManagementService
                .GetByIdAsync(
                    brandId ,
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
        SystemPermissions.Brands.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBrandRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _brandManagementService
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
                brandId =
                    result.Response.Id
            } ,
            result.Response);
    }

    [RequirePermission(
        SystemPermissions.Brands.Edit)]
    [HttpPut("{brandId:int}")]
    public async Task<IActionResult> Update(
        int brandId ,
        [FromBody] UpdateBrandRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _brandManagementService
                .UpdateAsync(
                    actorUserId ,
                    brandId ,
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
        SystemPermissions.Brands.Delete)]
    [HttpDelete("{brandId:int}")]
    public async Task<IActionResult> Delete(
        int brandId ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _brandManagementService
                .DeleteAsync(
                    actorUserId ,
                    brandId ,
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
        SystemPermissions.Brands.Restore)]
    [HttpPost("{brandId:int}/restore")]
    public async Task<IActionResult> Restore(
        int brandId ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _brandManagementService
                .RestoreAsync(
                    actorUserId ,
                    brandId ,
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
                BrandManagementErrorCodes.NotFound =>
                    StatusCodes.Status404NotFound,

                BrandManagementErrorCodes.DuplicateName =>
                    StatusCodes.Status409Conflict,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,
                Title =
                    "Brand management request failed." ,
                Detail =
                    errorMessage
                    ?? "The brand management request could not be completed."
            };

        problemDetails.Extensions["code"] =
            errorCode
            ?? BrandManagementErrorCodes.InvalidRequest;

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
                    "Brand management authentication failed." ,
                Detail =
                    "The authenticated dashboard account is invalid."
            };

        problemDetails.Extensions["code"] =
            BrandManagementErrorCodes.InvalidRequest;

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
                    "Brand management response failed." ,
                Detail =
                    "The brand operation succeeded, but its response could not be returned."
            };

        problemDetails.Extensions["code"] =
            "brands.response_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}
