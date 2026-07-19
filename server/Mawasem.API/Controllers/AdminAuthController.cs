using Mawasem.API.Authentication;
using Mawasem.API.Authorization;
using Mawasem.API.Extensions;
using Mawasem.Application.Features.Authentication.Contracts.Requests;
using Mawasem.Application.Features.Authentication.Contracts.Responses;
using Mawasem.Application.Features.Authentication.Interfaces;
using Mawasem.Application.Features.Authentication.Models;
using Mawasem.Application.Features.Authentication.Options;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/admin/auth")]
public sealed class AdminAuthController : ControllerBase
{
    private const string RefreshTokenCookiePath =
        "/api/admin/auth";

    private readonly IDashboardAuthenticationService
        _authenticationService;

    private readonly IDashboardUserProfileService
        _userProfileService;

    private readonly JwtSettings _jwtSettings;

    private readonly TimeProvider _timeProvider;

    public AdminAuthController(
        IDashboardAuthenticationService authenticationService ,
        IDashboardUserProfileService userProfileService ,
        IOptions<JwtSettings> jwtOptions ,
        TimeProvider timeProvider )
    {
        _authenticationService = authenticationService;
        _userProfileService = userProfileService;
        _jwtSettings = jwtOptions.Value;
        _timeProvider = timeProvider;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginAdminRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _authenticationService.LoginAsync(
                request ,
                GetClientIpAddress() ,
                cancellationToken);

        if ( !result.Succeeded )
        {
            return CreateSessionFailureResponse(result);
        }

        if ( result.Response is null ||
            !TryWriteAuthenticationCookies(
                result.Response ,
                result.RefreshToken) )
        {
            DeleteAuthenticationCookies();

            return CreateUnexpectedSessionFailureResponse();
        }

        return Ok(result.Response);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        CancellationToken cancellationToken )
    {
        Request.Cookies.TryGetValue(
            AuthenticationCookieNames.DashboardRefreshToken ,
            out var refreshToken);

        var result =
            await _authenticationService.RefreshAsync(
                refreshToken ,
                GetClientIpAddress() ,
                cancellationToken);

        if ( !result.Succeeded )
        {
            DeleteAuthenticationCookies();

            return CreateSessionFailureResponse(result);
        }

        if ( result.Response is null ||
            !TryWriteAuthenticationCookies(
                result.Response ,
                result.RefreshToken) )
        {
            DeleteAuthenticationCookies();

            return CreateUnexpectedSessionFailureResponse();
        }

        return Ok(result.Response);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken )
    {
        Request.Cookies.TryGetValue(
            AuthenticationCookieNames.DashboardRefreshToken ,
            out var refreshToken);

        await _authenticationService.LogoutAsync(
            refreshToken ,
            GetClientIpAddress() ,
            cancellationToken);

        DeleteAuthenticationCookies();

        return NoContent();
    }

    [RequirePermission(
        SystemPermissions.Dashboard.Access)]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(
        CancellationToken cancellationToken )
    {
        if ( !User.TryGetUserId(
                out var userId) )
        {
            return CreateProfileFailureResponse(
                DashboardUserProfileResult.Failure(
                    AuthenticationErrorCodes.InvalidCredentials ,
                    "The authenticated dashboard account is invalid."));
        }

        var result =
            await _userProfileService.GetAsync(
                userId ,
                cancellationToken);

        if ( !result.Succeeded )
        {
            return CreateProfileFailureResponse(result);
        }

        if ( result.User is null )
        {
            var problemDetails =
                new ProblemDetails
                {
                    Status =
                        StatusCodes.Status500InternalServerError ,

                    Title =
                        "Dashboard profile retrieval failed." ,

                    Detail =
                        "The dashboard profile could not be returned."
                };

            problemDetails.Extensions["code"] =
                "authentication.dashboard_profile_failed";

            return StatusCode(
                StatusCodes.Status500InternalServerError ,
                problemDetails);
        }

        return Ok(result.User);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangeDashboardPasswordRequest request ,
        CancellationToken cancellationToken )
    {
        if ( !User.TryGetUserId(
                out var userId) )
        {
            return CreateOperationFailureResponse(
                DashboardAuthenticationOperationResult.Failure(
                    AuthenticationErrorCodes.InvalidCredentials ,
                    "The authenticated dashboard account is invalid."));
        }

        var result =
            await _authenticationService.ChangePasswordAsync(
                userId ,
                request ,
                GetClientIpAddress() ,
                cancellationToken);

        if ( !result.Succeeded )
        {
            return CreateOperationFailureResponse(result);
        }

        DeleteAuthenticationCookies();

        return NoContent();
    }

    private bool TryWriteAuthenticationCookies(
        DashboardAuthenticationResponse response ,
        string? refreshToken )
    {
        if ( string.IsNullOrWhiteSpace(response.AccessToken) ||
            response.AccessTokenExpiresAtUtc == default ||
            string.IsNullOrWhiteSpace(refreshToken) )
        {
            return false;
        }

        Response.Cookies.Append(
            AuthenticationCookieNames.AccessToken ,
            response.AccessToken ,
            AuthenticationCookieOptionsFactory
                .CreateAccessToken(
                    response.AccessTokenExpiresAtUtc));

        WriteRefreshTokenCookie(refreshToken);

        return true;
    }

    private void WriteRefreshTokenCookie(
        string refreshToken )
    {
        var expiresAt =
            _timeProvider
                .GetUtcNow()
                .AddDays(_jwtSettings.RefreshTokenDays);

        Response.Cookies.Append(
            AuthenticationCookieNames.DashboardRefreshToken ,
            refreshToken ,
            AuthenticationCookieOptionsFactory
                .CreateRefreshToken(
                    expiresAt ,
                    RefreshTokenCookiePath));
    }

    private void DeleteAuthenticationCookies()
    {
        Response.Cookies.Delete(
            AuthenticationCookieNames.AccessToken ,
            AuthenticationCookieOptionsFactory
                .CreateDeletion("/"));

        Response.Cookies.Delete(
            AuthenticationCookieNames.DashboardRefreshToken ,
            AuthenticationCookieOptionsFactory
                .CreateDeletion(
                    RefreshTokenCookiePath));
    }

    private string? GetClientIpAddress()
    {
        return HttpContext
            .Connection
            .RemoteIpAddress?
            .ToString();
    }

    private IActionResult CreateSessionFailureResponse(
        DashboardAuthenticationSessionResult result )
    {
        var statusCode =
            result.ErrorCode switch
            {
                AuthenticationErrorCodes.InvalidRequest =>
                    StatusCodes.Status400BadRequest,

                AuthenticationErrorCodes.InvalidCredentials =>
                    StatusCodes.Status401Unauthorized,

                AuthenticationErrorCodes.InvalidRefreshToken =>
                    StatusCodes.Status401Unauthorized,

                AuthenticationErrorCodes.AccountBlocked =>
                    StatusCodes.Status403Forbidden,

                AuthenticationErrorCodes.AccountLocked =>
                    StatusCodes.Status423Locked,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,

                Title =
                    "Dashboard authentication request failed." ,

                Detail =
                    result.ErrorMessage
                    ?? "The dashboard authentication request could not be completed."
            };

        problemDetails.Extensions["code"] =
            result.ErrorCode
            ?? AuthenticationErrorCodes.InvalidRequest;

        return StatusCode(
            statusCode ,
            problemDetails);
    }

    private IActionResult CreateOperationFailureResponse(
        DashboardAuthenticationOperationResult result )
    {
        var statusCode =
            result.ErrorCode switch
            {
                AuthenticationErrorCodes.InvalidRequest =>
                    StatusCodes.Status400BadRequest,

                AuthenticationErrorCodes
                    .PasswordConfirmationMismatch =>
                    StatusCodes.Status400BadRequest,

                AuthenticationErrorCodes
                    .CurrentPasswordIncorrect =>
                    StatusCodes.Status400BadRequest,

                AuthenticationErrorCodes.PasswordChangeFailed =>
                    StatusCodes.Status400BadRequest,

                AuthenticationErrorCodes.InvalidCredentials =>
                    StatusCodes.Status401Unauthorized,

                AuthenticationErrorCodes.AccountBlocked =>
                    StatusCodes.Status403Forbidden,

                AuthenticationErrorCodes.AccountLocked =>
                    StatusCodes.Status423Locked,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,

                Title =
                    "Dashboard account operation failed." ,

                Detail =
                    result.ErrorMessage
                    ?? "The dashboard account operation could not be completed."
            };

        problemDetails.Extensions["code"] =
            result.ErrorCode
            ?? AuthenticationErrorCodes.InvalidRequest;

        return StatusCode(
            statusCode ,
            problemDetails);
    }

    private IActionResult CreateProfileFailureResponse(
        DashboardUserProfileResult result )
    {
        var statusCode =
            result.ErrorCode switch
            {
                AuthenticationErrorCodes.AccountBlocked =>
                    StatusCodes.Status403Forbidden,

                _ =>
                    StatusCodes.Status401Unauthorized
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,

                Title =
                    "Dashboard profile request failed." ,

                Detail =
                    result.ErrorMessage
                    ?? "The dashboard profile could not be retrieved."
            };

        problemDetails.Extensions["code"] =
            result.ErrorCode
            ?? AuthenticationErrorCodes.InvalidCredentials;

        return StatusCode(
            statusCode ,
            problemDetails);
    }

    private IActionResult CreateUnexpectedSessionFailureResponse()
    {
        var problemDetails =
            new ProblemDetails
            {
                Status =
                    StatusCodes.Status500InternalServerError ,

                Title =
                    "Dashboard authentication session creation failed." ,

                Detail =
                    "The account operation succeeded, but the " +
                    "dashboard authentication session could not be created."
            };

        problemDetails.Extensions["code"] =
            "authentication.dashboard_session_creation_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}