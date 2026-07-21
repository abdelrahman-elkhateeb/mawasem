using Mawasem.API.Authorization;
using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Interfaces;
using Mawasem.Application.Features.Products.Models;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Mawasem.API.Controllers;

[ApiController]
[Route(
    "api/admin/products/{productId:int}/variants")]
public sealed class AdminProductVariantsController
    : ControllerBase
{
    private readonly IProductVariantManagementService
        _productVariantManagementService;

    public AdminProductVariantsController(
        IProductVariantManagementService
            productVariantManagementService )
    {
        ArgumentNullException.ThrowIfNull(
            productVariantManagementService);

        _productVariantManagementService =
            productVariantManagementService;
    }

    [RequirePermission(
        SystemPermissions.Products.View)]
    [HttpGet]
    public async Task<IActionResult> GetByProductId(
        int productId ,
        CancellationToken cancellationToken )
    {
        var result =
            await _productVariantManagementService
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
        SystemPermissions.Products.Edit)]
    [HttpPost]
    public async Task<IActionResult> Create(
        int productId ,
        [FromBody] CreateProductVariantRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _productVariantManagementService
                .CreateAsync(
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
        SystemPermissions.Products.Edit)]
    [HttpPut("{variantId:int}/availability")]
    public async Task<IActionResult> UpdateAvailability(
        int productId ,
        int variantId ,
        [FromBody]
        UpdateProductVariantAvailabilityRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _productVariantManagementService
                .UpdateAvailabilityAsync(
                    productId ,
                    variantId ,
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
        SystemPermissions.Products.ManageStock)]
    [HttpPut("{variantId:int}/stock")]
    public async Task<IActionResult> UpdateStock(
        int productId ,
        int variantId ,
        [FromBody]
        UpdateProductVariantStockRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _productVariantManagementService
                .UpdateStockAsync(
                    productId ,
                    variantId ,
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

    private IActionResult CreateFailureResponse(
        string? errorCode ,
        string? errorMessage )
    {
        var statusCode =
            errorCode switch
            {
                ProductVariantManagementErrorCodes
                    .ProductNotFound =>
                    StatusCodes.Status404NotFound,

                ProductVariantManagementErrorCodes
                    .VariantNotFound =>
                    StatusCodes.Status404NotFound,

                ProductVariantManagementErrorCodes
                    .OptionValueNotFound =>
                    StatusCodes.Status404NotFound,

                ProductVariantManagementErrorCodes
                    .CombinationAlreadyExists =>
                    StatusCodes.Status409Conflict,

                ProductVariantManagementErrorCodes
                    .InconsistentOptionStructure =>
                    StatusCodes.Status409Conflict,

                ProductVariantManagementErrorCodes
                    .StockConcurrencyConflict =>
                    StatusCodes.Status409Conflict,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,
                Title =
                    "Product variant management request failed." ,
                Detail =
                    errorMessage ??
                    "The product variant request could not be completed."
            };

        problemDetails.Extensions["code"] =
            errorCode ??
            ProductManagementErrorCodes.InvalidRequest;

        return StatusCode(
            statusCode ,
            problemDetails);
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
                    "Product variant management response failed." ,

                Detail =
                    "The operation succeeded, but no product variant response was returned."
            };

        problemDetails.Extensions["code"] =
            "product_variants.response_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}