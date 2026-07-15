using Mawasem.API.Authorization;
using Mawasem.Application.Features.Customers.Contracts.Requests;
using Mawasem.Application.Features.Customers.Interfaces;
using Mawasem.Application.Features.Customers.Models;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/admin/customers")]
public sealed class AdminCustomersController : ControllerBase
{
    private readonly ICustomerManagementService
        _customerManagementService;

    public AdminCustomersController(
        ICustomerManagementService customerManagementService )
    {
        _customerManagementService =
            customerManagementService;
    }

    [RequirePermission(
        SystemPermissions.Customers.View)]
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] GetCustomersRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _customerManagementService
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
        SystemPermissions.Customers.View)]
    [HttpGet("{customerId:int}")]
    public async Task<IActionResult> GetById(
        int customerId ,
        CancellationToken cancellationToken )
    {
        var result =
            await _customerManagementService
                .GetByIdAsync(
                    customerId ,
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
                CustomerManagementErrorCodes.NotFound =>
                    StatusCodes.Status404NotFound,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,
                Title =
                    "Customer management request failed." ,
                Detail =
                    errorMessage
                    ?? "The customer management request could not be completed."
            };

        problemDetails.Extensions["code"] =
            errorCode
            ?? CustomerManagementErrorCodes.InvalidRequest;

        return StatusCode(
            statusCode ,
            problemDetails);
    }

    private IActionResult CreateUnexpectedFailureResponse()
    {
        var problemDetails =
            new ProblemDetails
            {
                Status =
                    StatusCodes.Status500InternalServerError ,
                Title =
                    "Customer management response failed." ,
                Detail =
                    "The customer operation succeeded, but its response could not be returned."
            };

        problemDetails.Extensions["code"] =
            "customers.response_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}