using Mawasem.API.Authorization;
using Mawasem.Application.Features.Employees.Contracts.Requests;
using Mawasem.Application.Features.Employees.Interfaces;
using Mawasem.Application.Features.Employees.Models;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/admin/employees")]
public sealed class AdminEmployeesController : ControllerBase
{
    private readonly IEmployeeManagementService
        _employeeManagementService;

    public AdminEmployeesController(
        IEmployeeManagementService employeeManagementService )
    {
        _employeeManagementService =
            employeeManagementService;
    }

    [RequirePermission(
        SystemPermissions.Employees.View)]
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] GetEmployeesRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _employeeManagementService
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
        SystemPermissions.Employees.View)]
    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> GetById(
        int employeeId ,
        CancellationToken cancellationToken )
    {
        var result =
            await _employeeManagementService
                .GetByIdAsync(
                    employeeId ,
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
        SystemPermissions.Employees.View)]
    [HttpGet("access-options")]
    public async Task<IActionResult> GetAccessOptions(
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _employeeManagementService
                .GetAccessOptionsAsync(
                    actorUserId ,
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
        SystemPermissions.Employees.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _employeeManagementService
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
                employeeId =
                    result.Response.Id
            } ,
            result.Response);
    }

    [RequirePermission(
        SystemPermissions.Employees.Edit)]
    [HttpPut("{employeeId:int}")]
    public async Task<IActionResult> Update(
        int employeeId ,
        [FromBody] UpdateEmployeeRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _employeeManagementService
                .UpdateAsync(
                    actorUserId ,
                    employeeId ,
                    request ,
                    GetClientIpAddress() ,
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
        SystemPermissions.Employees.Block)]
    [HttpPost("{employeeId:int}/block")]
    public async Task<IActionResult> Block(
        int employeeId ,
        [FromBody] BlockEmployeeRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _employeeManagementService
                .BlockAsync(
                    actorUserId ,
                    employeeId ,
                    request ,
                    GetClientIpAddress() ,
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
        SystemPermissions.Employees.Unblock)]
    [HttpPost("{employeeId:int}/unblock")]
    public async Task<IActionResult> Unblock(
        int employeeId ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _employeeManagementService
                .UnblockAsync(
                    actorUserId ,
                    employeeId ,
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
        SystemPermissions.Employees.ResetPassword)]
    [HttpPost("{employeeId:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        int employeeId ,
        [FromBody] ResetEmployeePasswordRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _employeeManagementService
                .ResetPasswordAsync(
                    actorUserId ,
                    employeeId ,
                    request ,
                    GetClientIpAddress() ,
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
        SystemPermissions.Employees.AssignRoles)]
    [HttpPut("{employeeId:int}/roles")]
    public async Task<IActionResult> AssignRoles(
        int employeeId ,
        [FromBody] AssignEmployeeRolesRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _employeeManagementService
                .AssignRolesAsync(
                    actorUserId ,
                    employeeId ,
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
        SystemPermissions.Employees.AssignPermissions)]
    [HttpPut("{employeeId:int}/permissions")]
    public async Task<IActionResult> AssignPermissions(
        int employeeId ,
        [FromBody] AssignEmployeePermissionsRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _employeeManagementService
                .AssignPermissionsAsync(
                    actorUserId ,
                    employeeId ,
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

    private bool TryGetActorUserId(
        out int actorUserId )
    {
        var userIdValue =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(
            userIdValue ,
            NumberStyles.None ,
            CultureInfo.InvariantCulture ,
            out actorUserId);
    }

    private string? GetClientIpAddress()
    {
        return HttpContext
            .Connection
            .RemoteIpAddress?
            .ToString();
    }

    private IActionResult CreateFailureResponse(
        string? errorCode ,
        string? errorMessage )
    {
        var statusCode =
            errorCode switch
            {
                EmployeeManagementErrorCodes.NotFound =>
                    StatusCodes.Status404NotFound,

                EmployeeManagementErrorCodes
                    .EmailAlreadyRegistered =>
                    StatusCodes.Status409Conflict,

                EmployeeManagementErrorCodes.Forbidden =>
                    StatusCodes.Status403Forbidden,

                EmployeeManagementErrorCodes
                    .CannotManageSuperAdmin =>
                    StatusCodes.Status403Forbidden,

                EmployeeManagementErrorCodes
                    .CannotManageSelf =>
                    StatusCodes.Status403Forbidden,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,
                Title =
                    "Employee management request failed." ,
                Detail =
                    errorMessage
                    ?? "The employee management request could not be completed."
            };

        problemDetails.Extensions["code"] =
            errorCode
            ?? EmployeeManagementErrorCodes.InvalidRequest;

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
                    "Employee management authentication failed." ,
                Detail =
                    "The authenticated dashboard account is invalid."
            };

        problemDetails.Extensions["code"] =
            EmployeeManagementErrorCodes.Forbidden;

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
                    "Employee management response failed." ,
                Detail =
                    "The employee operation succeeded, but its response could not be returned."
            };

        problemDetails.Extensions["code"] =
            "employees.response_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}