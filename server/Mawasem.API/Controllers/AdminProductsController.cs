using Mawasem.API.Authorization;
using Mawasem.API.Extensions;
using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Interfaces;
using Mawasem.Application.Features.Products.Models;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/admin/products")]
public sealed class AdminProductsController : ControllerBase
{
    private readonly IProductManagementService
        _productManagementService;

    public AdminProductsController(
        IProductManagementService productManagementService )
    {
        _productManagementService =
            productManagementService;
    }

    [RequirePermission(
        SystemPermissions.Products.View)]
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] GetProductsRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _productManagementService
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
        SystemPermissions.Products.View)]
    [HttpGet("{productId:int}")]
    public async Task<IActionResult> GetById(
        int productId ,
        CancellationToken cancellationToken )
    {
        var result =
            await _productManagementService
                .GetByIdAsync(
                    productId ,
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
        SystemPermissions.Products.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _productManagementService
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
                productId =
                    result.Response.Id
            } ,
            result.Response);
    }

    [RequirePermission(
        SystemPermissions.Products.Edit)]
    [HttpPut("{productId:int}")]
    public async Task<IActionResult> Update(
        int productId ,
        [FromBody] UpdateProductRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _productManagementService
                .UpdateAsync(
                    actorUserId ,
                    productId ,
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
        SystemPermissions.Products.Edit)]
    [HttpPut("{productId:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int productId ,
        [FromBody] UpdateProductStatusRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _productManagementService
                .UpdateStatusAsync(
                    actorUserId ,
                    productId ,
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
        SystemPermissions.Products.Delete)]
    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Delete(
        int productId ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _productManagementService
                .DeleteAsync(
                    actorUserId ,
                    productId ,
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
        SystemPermissions.Products.Restore)]
    [HttpPost("{productId:int}/restore")]
    public async Task<IActionResult> Restore(
        int productId ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _productManagementService
                .RestoreAsync(
                    actorUserId ,
                    productId ,
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
                ProductManagementErrorCodes.NotFound =>
                    StatusCodes.Status404NotFound,

                ProductManagementErrorCodes.DuplicateSlug =>
                    StatusCodes.Status409Conflict,

                ProductManagementErrorCodes.CannotPublish =>
                    StatusCodes.Status409Conflict,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,
                Title =
                    "Product management request failed." ,
                Detail =
                    errorMessage
                    ?? "The product management request could not be completed."
            };

        problemDetails.Extensions["code"] =
            errorCode
            ?? ProductManagementErrorCodes.InvalidRequest;

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
                    "Product management authentication failed." ,
                Detail =
                    "The authenticated dashboard account is invalid."
            };

        problemDetails.Extensions["code"] =
            ProductManagementErrorCodes.InvalidRequest;

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
                    "Product management response failed." ,
                Detail =
                    "The product operation succeeded, but its response could not be returned."
            };

        problemDetails.Extensions["code"] =
            "products.response_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}