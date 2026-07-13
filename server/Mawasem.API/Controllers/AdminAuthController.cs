using Mawasem.Application.Features.Authentication.Contracts.Requests;
using Mawasem.Application.Features.Authentication.Interfaces;
using Mawasem.Application.Features.Authentication.Models;
using Mawasem.Application.Features.Authentication.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/admin/auth")]
public sealed class AdminAuthController : ControllerBase
{
    private const string RefreshTokenCookieName =
        "mawasem_dashboard_refresh_token";

    private readonly IDashboardAuthenticationService
        _authenticationService;

    private readonly JwtSettings _jwtSettings;
    private readonly TimeProvider _timeProvider;

    public AdminAuthController(
        IDashboardAuthenticationService authenticationService ,
        IOptions<JwtSettings> jwtOptions ,
        TimeProvider timeProvider )
    {
        _authenticationService = authenticationService;
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
            return CreateFailureResponse(result);
        }

        if ( result.Response is null ||
            string.IsNullOrWhiteSpace(result.RefreshToken) )
        {
            return CreateUnexpectedFailureResponse();
        }

        WriteRefreshTokenCookie(result.RefreshToken);

        return Ok(result.Response);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        CancellationToken cancellationToken )
    {
        Request.Cookies.TryGetValue(
            RefreshTokenCookieName ,
            out var refreshToken);

        var result =
            await _authenticationService.RefreshAsync(
                refreshToken ,
                GetClientIpAddress() ,
                cancellationToken);

        if ( !result.Succeeded )
        {
            DeleteRefreshTokenCookie();

            return CreateFailureResponse(result);
        }

        if ( result.Response is null ||
            string.IsNullOrWhiteSpace(result.RefreshToken) )
        {
            DeleteRefreshTokenCookie();

            return CreateUnexpectedFailureResponse();
        }

        WriteRefreshTokenCookie(result.RefreshToken);

        return Ok(result.Response);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken )
    {
        Request.Cookies.TryGetValue(
            RefreshTokenCookieName ,
            out var refreshToken);

        await _authenticationService.LogoutAsync(
            refreshToken ,
            GetClientIpAddress() ,
            cancellationToken);

        DeleteRefreshTokenCookie();

        return NoContent();
    }

    private void WriteRefreshTokenCookie(
        string refreshToken )
    {
        var expiresAt =
            _timeProvider
                .GetUtcNow()
                .AddDays(_jwtSettings.RefreshTokenDays);

        Response.Cookies.Append(
            RefreshTokenCookieName ,
            refreshToken ,
            new CookieOptions
            {
                HttpOnly = true ,

                // Supports local HTTP development.
                // Production must use HTTPS.
                Secure = Request.IsHttps ,

                SameSite = SameSiteMode.Lax ,
                IsEssential = true ,
                Expires = expiresAt ,
                Path = "/api/admin/auth"
            });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(
            RefreshTokenCookieName ,
            new CookieOptions
            {
                HttpOnly = true ,
                Secure = Request.IsHttps ,
                SameSite = SameSiteMode.Lax ,
                IsEssential = true ,
                Path = "/api/admin/auth"
            });
    }

    private string? GetClientIpAddress()
    {
        return HttpContext
            .Connection
            .RemoteIpAddress?
            .ToString();
    }

    private IActionResult CreateFailureResponse(
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
                    423,

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

    private IActionResult CreateUnexpectedFailureResponse()
    {
        var problemDetails =
            new ProblemDetails
            {
                Status =
                    StatusCodes.Status500InternalServerError ,

                Title =
                    "Dashboard authentication session creation failed." ,

                Detail =
                    "The login succeeded, but the dashboard authentication session could not be created."
            };

        problemDetails.Extensions["code"] =
            "authentication.session_creation_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}