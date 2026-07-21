using Mawasem.API.Authorization;
using Mawasem.API.Extensions;
using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Interfaces;
using Mawasem.Application.Features.Products.Models;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Mawasem.API.Controllers;

[ApiController]
[Route(
    "api/admin/products/{productId:int}/images")]
public sealed class AdminProductImagesController
    : ControllerBase
{
    private readonly IProductImageManagementService
        _productImageManagementService;

    public AdminProductImagesController(
        IProductImageManagementService
            productImageManagementService )
    {
        ArgumentNullException.ThrowIfNull(
            productImageManagementService);

        _productImageManagementService =
            productImageManagementService;
    }

    [RequirePermission(
        SystemPermissions.Products.View)]
    [HttpGet]
    public async Task<IActionResult> GetByProductId(
        int productId ,
        CancellationToken cancellationToken )
    {
        var result =
            await _productImageManagementService
                .GetByProductIdAsync(
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
        SystemPermissions.Products.ManageImages)]
    [Consumes("multipart/form-data")]
    [HttpPost]
    public async Task<IActionResult> Upload(
        int productId ,
        IFormFile image ,
        [FromForm] int? colorOptionValueId ,
        [FromForm] bool isPrimary ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        if ( image is null )
        {
            return CreateFailureResponse(
                ProductManagementErrorCodes.InvalidImage ,
                "Select an image file.");
        }

        await using var content =
            image.OpenReadStream();

        var request =
            new UploadProductImageRequest
            {
                ColorOptionValueId =
                    colorOptionValueId ,

                IsPrimary = isPrimary ,
                Content = content ,
                FileName = image.FileName ,
                ContentType = image.ContentType ,
                Length = image.Length
            };

        var result =
            await _productImageManagementService
                .UploadAsync(
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

        return StatusCode(
            StatusCodes.Status201Created ,
            result.Response);
    }

    [RequirePermission(
        SystemPermissions.Products.ManageImages)]
    [HttpPut("{imageId:int}/primary")]
    public async Task<IActionResult> SetPrimary(
        int productId ,
        int imageId ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _productImageManagementService
                .SetPrimaryAsync(
                    actorUserId ,
                    productId ,
                    imageId ,
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
        SystemPermissions.Products.ManageImages)]
    [HttpPut("order")]
    public async Task<IActionResult> Reorder(
        int productId ,
        [FromBody] ReorderProductImagesRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _productImageManagementService
                .ReorderAsync(
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
        SystemPermissions.Products.ManageImages)]
    [HttpDelete("{imageId:int}")]
    public async Task<IActionResult> Delete(
        int productId ,
        int imageId ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _productImageManagementService
                .DeleteAsync(
                    actorUserId ,
                    productId ,
                    imageId ,
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

                ProductManagementErrorCodes.ImageNotFound =>
                    StatusCodes.Status404NotFound,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,
                Title =
                    "Product image management request failed." ,

                Detail =
                    errorMessage ??
                    "The product image request could not be completed."
            };

        problemDetails.Extensions["code"] =
            errorCode ??
            ProductManagementErrorCodes.InvalidRequest;

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
                    "Product image management authentication failed." ,

                Detail =
                    "The authenticated dashboard account is invalid."
            };

        problemDetails.Extensions["code"] =
            ProductManagementErrorCodes.InvalidRequest;

        return Unauthorized(problemDetails);
    }

    private IActionResult
        CreateUnexpectedFailureResponse()
    {
        var problemDetails =
            new ProblemDetails
            {
                Status =
                    StatusCodes
                        .Status500InternalServerError ,

                Title =
                    "Product image management response failed." ,

                Detail =
                    "The operation succeeded, but no product image response was returned."
            };

        problemDetails.Extensions["code"] =
            "product_images.response_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}