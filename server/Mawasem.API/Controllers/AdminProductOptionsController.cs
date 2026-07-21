using Mawasem.API.Authorization;
using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Interfaces;
using Mawasem.Application.Features.Products.Models;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/admin/product-options")]
public sealed class AdminProductOptionsController
    : ControllerBase
{
    private readonly IProductOptionManagementService
        _productOptionManagementService;

    public AdminProductOptionsController(
        IProductOptionManagementService
            productOptionManagementService )
    {
        ArgumentNullException.ThrowIfNull(
            productOptionManagementService);

        _productOptionManagementService =
            productOptionManagementService;
    }

    [RequirePermission(
        SystemPermissions.Products.View)]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken )
    {
        var result =
            await _productOptionManagementService
                .GetAllAsync(cancellationToken);

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
        [FromBody] CreateProductOptionRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _productOptionManagementService
                .CreateAsync(
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
    [HttpPut("{optionId:int}")]
    public async Task<IActionResult> Update(
        int optionId ,
        [FromBody] UpdateProductOptionRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _productOptionManagementService
                .UpdateAsync(
                    optionId ,
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
    [HttpPost("{optionId:int}/values")]
    public async Task<IActionResult> CreateValue(
        int optionId ,
        [FromBody] CreateProductOptionValueRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _productOptionManagementService
                .CreateValueAsync(
                    optionId ,
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
    [HttpPut(
        "{optionId:int}/values/{valueId:int}")]
    public async Task<IActionResult> UpdateValue(
        int optionId ,
        int valueId ,
        [FromBody] UpdateProductOptionValueRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _productOptionManagementService
                .UpdateValueAsync(
                    optionId ,
                    valueId ,
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
                ProductOptionManagementErrorCodes
                    .OptionNotFound =>
                    StatusCodes.Status404NotFound,

                ProductOptionManagementErrorCodes
                    .OptionValueNotFound =>
                    StatusCodes.Status404NotFound,

                ProductOptionManagementErrorCodes
                    .OptionValueDoesNotBelongToOption =>
                    StatusCodes.Status409Conflict,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,
                Title =
                    "Product option management request failed." ,
                Detail =
                    errorMessage ??
                    "The product option request could not be completed."
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
                    "Product option management response failed." ,

                Detail =
                    "The operation succeeded, but no product option response was returned."
            };

        problemDetails.Extensions["code"] =
            "product_options.response_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}