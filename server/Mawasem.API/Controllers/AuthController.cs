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

    private readonly ICustomerPasswordResetService
        _passwordResetService;

    private readonly JwtSettings _jwtSettings;

    private readonly TimeProvider _timeProvider;

    public AuthController(
        ICustomerAuthenticationService authenticationService ,
        ICustomerPasswordResetService passwordResetService ,
        IOptions<JwtSettings> jwtOptions ,
        TimeProvider timeProvider )
    {
        _authenticationService = authenticationService;
        _passwordResetService = passwordResetService;
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
            return CreateAuthenticationFailureResponse(result);
        }

        if ( result.Response is null ||
            string.IsNullOrWhiteSpace(result.RefreshToken) )
        {
            return CreateUnexpectedSessionFailureResponse();
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
            return CreateAuthenticationFailureResponse(result);
        }

        if ( result.Response is null ||
            string.IsNullOrWhiteSpace(result.RefreshToken) )
        {
            return CreateUnexpectedSessionFailureResponse();
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

            return CreateAuthenticationFailureResponse(result);
        }

        if ( result.Response is null ||
            string.IsNullOrWhiteSpace(result.RefreshToken) )
        {
            DeleteRefreshTokenCookie();

            return CreateUnexpectedSessionFailureResponse();
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

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotCustomerPasswordRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _passwordResetService.RequestCodeAsync(
                request ,
                GetClientIpAddress() ,
                cancellationToken);

        if ( !result.Succeeded )
        {
            return CreatePasswordResetFailureResponse(result);
        }

        // The same response is returned whether the account exists
        // or not, preventing phone-number account discovery.
        return Accepted();
    }

    [AllowAnonymous]
    [HttpPost("verify-reset-code")]
    public async Task<IActionResult> VerifyResetCode(
        [FromBody] VerifyCustomerPasswordResetCodeRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _passwordResetService.VerifyCodeAsync(
                request ,
                cancellationToken);

        if ( !result.Succeeded )
        {
            return CreatePasswordResetVerificationFailureResponse(
                result);
        }

        if ( result.Response is null )
        {
            return CreateUnexpectedPasswordResetFailureResponse();
        }

        return Ok(result.Response);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetCustomerPasswordRequest request ,
        CancellationToken cancellationToken )
    {
        var result =
            await _passwordResetService.ResetPasswordAsync(
                request ,
                GetClientIpAddress() ,
                cancellationToken);

        if ( !result.Succeeded )
        {
            return CreatePasswordResetFailureResponse(result);
        }

        // Existing refresh tokens are revoked in the database.
        // Delete the local browser cookie as well.
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

                // Allows local HTTP development.
                // Production must run through HTTPS.
                Secure = Request.IsHttps ,

                SameSite = SameSiteMode.Lax ,
                IsEssential = true ,
                Expires = expiresAt ,
                Path = "/api/auth"
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

    private IActionResult CreateAuthenticationFailureResponse(
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

    private IActionResult CreatePasswordResetFailureResponse(
        CustomerPasswordResetOperationResult result )
    {
        var statusCode =
            result.ErrorCode switch
            {
                AuthenticationErrorCodes.InvalidRequest =>
                    StatusCodes.Status400BadRequest,

                AuthenticationErrorCodes.InvalidPhoneNumber =>
                    StatusCodes.Status400BadRequest,

                AuthenticationErrorCodes
                    .PasswordConfirmationMismatch =>
                    StatusCodes.Status400BadRequest,

                AuthenticationErrorCodes
                    .PasswordResetTokenInvalid =>
                    StatusCodes.Status400BadRequest,

                AuthenticationErrorCodes
                    .PasswordResetFailed =>
                    StatusCodes.Status400BadRequest,

                AuthenticationErrorCodes
                    .PasswordResetDeliveryFailed =>
                    StatusCodes.Status503ServiceUnavailable,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,
                Title = "Password reset request failed." ,

                Detail =
                    result.ErrorMessage
                    ?? "The password reset request could not be completed."
            };

        problemDetails.Extensions["code"] =
            result.ErrorCode
            ?? AuthenticationErrorCodes.InvalidRequest;

        return StatusCode(
            statusCode ,
            problemDetails);
    }

    private IActionResult
        CreatePasswordResetVerificationFailureResponse(
            CustomerPasswordResetVerificationResult result )
    {
        var statusCode =
            result.ErrorCode switch
            {
                AuthenticationErrorCodes
                    .PasswordResetCodeInvalid =>
                    StatusCodes.Status400BadRequest,

                _ =>
                    StatusCodes.Status400BadRequest
            };

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode ,

                Title =
                    "Password reset code verification failed." ,

                Detail =
                    result.ErrorMessage
                    ?? "The verification code could not be accepted."
            };

        problemDetails.Extensions["code"] =
            result.ErrorCode
            ?? AuthenticationErrorCodes
                .PasswordResetCodeInvalid;

        return StatusCode(
            statusCode ,
            problemDetails);
    }

    private IActionResult
        CreateUnexpectedSessionFailureResponse()
    {
        var problemDetails =
            new ProblemDetails
            {
                Status =
                    StatusCodes.Status500InternalServerError ,

                Title =
                    "Authentication session creation failed." ,

                Detail =
                    "The account operation succeeded, but the " +
                    "authentication session could not be created."
            };

        problemDetails.Extensions["code"] =
            "authentication.session_creation_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }

    private IActionResult
        CreateUnexpectedPasswordResetFailureResponse()
    {
        var problemDetails =
            new ProblemDetails
            {
                Status =
                    StatusCodes.Status500InternalServerError ,

                Title =
                    "Password reset verification failed." ,

                Detail =
                    "The verification operation succeeded, but " +
                    "the password reset token could not be returned."
            };

        problemDetails.Extensions["code"] =
            "authentication.password_reset_token_creation_failed";

        return StatusCode(
            StatusCodes.Status500InternalServerError ,
            problemDetails);
    }
}