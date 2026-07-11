using Mawasem.Application.Features.Authentication.Contracts.Requests;
using Mawasem.Application.Features.Authentication.Interfaces;
using Mawasem.Application.Features.Authentication.Models;
using Mawasem.Application.Features.Authentication.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName =
        "mawasem_customer_refresh_token";

    private readonly ICustomerAuthenticationService
        _authenticationService;

    private readonly JwtSettings _jwtSettings;
    private readonly TimeProvider _timeProvider;

    public AuthController(
        ICustomerAuthenticationService authenticationService ,
        IOptions<JwtSettings> jwtOptions ,
        TimeProvider timeProvider )
    {
        _authenticationService = authenticationService;
        _jwtSettings = jwtOptions.Value;
        _timeProvider = timeProvider;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCustomerRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _authenticationService.RegisterAsync(
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

        return StatusCode(
            StatusCodes.Status201Created ,
            result.Response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCustomerRequest request ,
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

                // Local HTTP development remains usable.
                // Production requests must use HTTPS.
                Secure = Request.IsHttps ,

                SameSite = SameSiteMode.Lax ,
                IsEssential = true ,

                Expires = expiresAt ,

                // Future refresh and logout endpoints will use this path.
                Path = "/api/auth"
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
        AuthenticationSessionResult result )
    {
        var statusCode =
            result.ErrorCode switch
            {
                AuthenticationErrorCodes.InvalidRequest =>
                    StatusCodes.Status400BadRequest,

                AuthenticationErrorCodes.InvalidPhoneNumber =>
                    StatusCodes.Status400BadRequest,

                AuthenticationErrorCodes.RegistrationFailed =>
                    StatusCodes.Status400BadRequest,

                AuthenticationErrorCodes.PhoneAlreadyRegistered =>
                    StatusCodes.Status409Conflict,

                AuthenticationErrorCodes.InvalidCredentials =>
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
                Title = "Authentication request failed." ,
                Detail =
                    result.ErrorMessage
                    ?? "The authentication request could not be completed."
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
                    "Authentication session creation failed." ,

                Detail =
                    "The account operation succeeded, but the authentication session could not be created."
            };

        problemDetails.Extensions["code"] =
            "authentication.session_creation_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}