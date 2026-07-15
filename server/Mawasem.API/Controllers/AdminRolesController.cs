using Mawasem.API.Authorization;
using Mawasem.Application.Features.Roles.Contracts.Requests;
using Mawasem.Application.Features.Roles.Interfaces;
using Mawasem.Application.Features.Roles.Models;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/admin/roles")]
public sealed class AdminRolesController : ControllerBase
{
    private readonly IRolePermissionManagementService
        _roleManagementService;

    public AdminRolesController(
        IRolePermissionManagementService roleManagementService )
    {
        _roleManagementService =
            roleManagementService;
    }

    [RequirePermission(
        SystemPermissions.Roles.View)]
    [HttpGet]
    public async Task<IActionResult> GetList(
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _roleManagementService
                .GetListAsync(
                    actorUserId ,
                    cancellationToken);

        return CreateResponse(result);
    }

    [RequirePermission(
        SystemPermissions.Roles.View)]
    [HttpGet("{roleName}")]
    public async Task<IActionResult> GetByName(
        string roleName ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _roleManagementService
                .GetByNameAsync(
                    actorUserId ,
                    roleName ,
                    cancellationToken);

        return CreateResponse(result);
    }

    [RequirePermission(
        SystemPermissions.Roles.View)]
    [HttpGet("permission-options")]
    public async Task<IActionResult> GetPermissionOptions(
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _roleManagementService
                .GetPermissionOptionsAsync(
                    actorUserId ,
                    cancellationToken);

        return CreateResponse(result);
    }

    [RequirePermission(
        SystemPermissions.Roles.ManagePermissions)]
    [HttpPut("{roleName}/permissions")]
    public async Task<IActionResult> UpdatePermissions(
        string roleName ,
        [FromBody] UpdateRolePermissionsRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !TryGetActorUserId(
                out var actorUserId) )
        {
            return CreateInvalidActorResponse();
        }

        var result =
            await _roleManagementService
                .UpdatePermissionsAsync(
                    actorUserId ,
                    roleName ,
                    request ,
                    cancellationToken);

        return CreateResponse(result);
    }

    private IActionResult CreateResponse<TResponse>(
        RoleManagementResult<TResponse> result )
        where TResponse : class
    {
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

    private IActionResult CreateFailureResponse(
        string? errorCode ,
        string? errorMessage )
    {
        var statusCode =
            errorCode switch
            {
                RoleManagementErrorCodes.NotFound =>
                    StatusCodes.Status404NotFound,

                RoleManagementErrorCodes.Forbidden =>
                    StatusCodes.Status403Forbidden,

                RoleManagementErrorCodes.ProtectedRole =>
                    StatusCodes.Status403Forbidden,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,
                Title =
                    "Role management request failed." ,
                Detail =
                    errorMessage
                    ?? "The role management request could not be completed."
            };

        problemDetails.Extensions["code"] =
            errorCode
            ?? RoleManagementErrorCodes.InvalidRequest;

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
                    "Role management authentication failed." ,
                Detail =
                    "The authenticated dashboard account is invalid."
            };

        problemDetails.Extensions["code"] =
            RoleManagementErrorCodes.Forbidden;

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
                    "Role management response failed." ,
                Detail =
                    "The role operation succeeded, but its response could not be returned."
            };

        problemDetails.Extensions["code"] =
            "roles.response_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}