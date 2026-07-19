using Mawasem.API.Controllers;
using Mawasem.Application.Features.Authentication.Contracts.Requests;
using Mawasem.Application.Features.Authentication.Contracts.Responses;
using Mawasem.Application.Features.Authentication.Interfaces;
using Mawasem.Application.Features.Authentication.Models;
using Mawasem.Application.Features.Authentication.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Mawasem.Tests.Authentication;

public sealed class HttpOnlyCookieAuthenticationTests
{
    [Fact]
    public async Task CustomerLogin_WritesSecureCookiesWithoutSerializingAccessToken()
    {
        var response =
            new AuthenticationResponse
            {
                AccessToken =
                    "customer-access-token" ,

                AccessTokenExpiresAtUtc =
                    new DateTime(
                        2026 ,
                        7 ,
                        20 ,
                        12 ,
                        0 ,
                        0 ,
                        DateTimeKind.Utc) ,

                User =
                    new AuthenticatedUserResponse
                    {
                        Id = 10 ,
                        FullNameAr = "عميل" ,
                        FullNameEn = "Customer" ,
                        PhoneNumber = "01000000000" ,
                        Email = "customer@example.com" ,
                        Roles = new[] { "Customer" }
                    }
            };

        var authenticationService =
            new CustomerAuthenticationServiceStub(
                AuthenticationSessionResult.Success(
                    response ,
                    "customer-refresh-token"));

        var controller =
            new AuthController(
                authenticationService ,
                userProfileService: null! ,
                passwordResetService: null! ,
                Options.Create(
                    new JwtSettings
                    {
                        RefreshTokenDays = 30
                    }) ,
                TimeProvider.System);

        var httpContext =
            new DefaultHttpContext();

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext = httpContext
            };

        var actionResult =
            await controller.Login(
                new LoginCustomerRequest() ,
                CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(
                actionResult);

        AssertResponseDoesNotExposeToken(
            okResult.Value);

        var cookies =
            GetSetCookieHeaders(httpContext);

        Assert.Equal(
            2 ,
            cookies.Length);

        AssertSecureHttpOnlyStrictCookie(
            Assert.Single(
                cookies.Where(cookie =>
                    cookie.StartsWith(
                        "accessToken=" ,
                        StringComparison.Ordinal))) ,
            expectedPath: "/");

        AssertSecureHttpOnlyStrictCookie(
            Assert.Single(
                cookies.Where(cookie =>
                    cookie.StartsWith(
                        "mawasem_customer_refresh_token=" ,
                        StringComparison.Ordinal))) ,
            expectedPath: "/api/auth");
    }

    [Fact]
    public async Task DashboardLogin_WritesSecureCookiesWithoutSerializingAccessToken()
    {
        var response =
            new DashboardAuthenticationResponse
            {
                AccessToken =
                    "dashboard-access-token" ,

                AccessTokenExpiresAtUtc =
                    new DateTime(
                        2026 ,
                        7 ,
                        20 ,
                        12 ,
                        0 ,
                        0 ,
                        DateTimeKind.Utc) ,

                User =
                    new DashboardAuthenticatedUserResponse
                    {
                        Id = 20 ,
                        FullNameAr = "مدير" ,
                        FullNameEn = "Administrator" ,
                        Email = "admin@example.com" ,
                        Roles = new[] { "Admin" } ,
                        Permissions =
                            new[]
                            {
                                "Dashboard.Access"
                            }
                    }
            };

        var authenticationService =
            new DashboardAuthenticationServiceStub(
                DashboardAuthenticationSessionResult.Success(
                    response ,
                    "dashboard-refresh-token"));

        var controller =
            new AdminAuthController(
                authenticationService ,
                userProfileService: null! ,
                Options.Create(
                    new JwtSettings
                    {
                        RefreshTokenDays = 30
                    }) ,
                TimeProvider.System);

        var httpContext =
            new DefaultHttpContext();

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext = httpContext
            };

        var actionResult =
            await controller.Login(
                new LoginAdminRequest() ,
                CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(
                actionResult);

        AssertResponseDoesNotExposeToken(
            okResult.Value);

        var cookies =
            GetSetCookieHeaders(httpContext);

        Assert.Equal(
            2 ,
            cookies.Length);

        AssertSecureHttpOnlyStrictCookie(
            Assert.Single(
                cookies.Where(cookie =>
                    cookie.StartsWith(
                        "accessToken=" ,
                        StringComparison.Ordinal))) ,
            expectedPath: "/");

        AssertSecureHttpOnlyStrictCookie(
            Assert.Single(
                cookies.Where(cookie =>
                    cookie.StartsWith(
                        "mawasem_dashboard_refresh_token=" ,
                        StringComparison.Ordinal))) ,
            expectedPath: "/api/admin/auth");
    }

    private static void AssertResponseDoesNotExposeToken(
        object? response )
    {
        Assert.NotNull(response);

        var json =
            JsonSerializer.Serialize(
                response ,
                new JsonSerializerOptions(
                    JsonSerializerDefaults.Web));

        using var document =
            JsonDocument.Parse(json);

        var root =
            document.RootElement;

        Assert.True(
            root.TryGetProperty(
                "user" ,
                out _));

        Assert.False(
            root.TryGetProperty(
                "tokenType" ,
                out _));

        Assert.False(
            root.TryGetProperty(
                "accessToken" ,
                out _));

        Assert.False(
            root.TryGetProperty(
                "accessTokenExpiresAtUtc" ,
                out _));

        Assert.Single(
            root.EnumerateObject().ToArray());
    }

    private static string[] GetSetCookieHeaders(
        DefaultHttpContext httpContext )
    {
        return httpContext
            .Response
            .Headers
            .SetCookie
            .Select(value =>
                value ?? string.Empty)
            .ToArray();
    }

    private static void AssertSecureHttpOnlyStrictCookie(
        string cookie ,
        string expectedPath )
    {
        var normalizedCookie =
            cookie.ToLowerInvariant();

        Assert.Contains(
            "httponly" ,
            normalizedCookie);

        Assert.Contains(
            "secure" ,
            normalizedCookie);

        Assert.Contains(
            "samesite=strict" ,
            normalizedCookie);

        Assert.Contains(
            $"path={expectedPath.ToLowerInvariant()}" ,
            normalizedCookie);
    }

    private sealed class CustomerAuthenticationServiceStub
        : ICustomerAuthenticationService
    {
        private readonly AuthenticationSessionResult
            _sessionResult;

        public CustomerAuthenticationServiceStub(
            AuthenticationSessionResult sessionResult )
        {
            _sessionResult = sessionResult;
        }

        public Task<AuthenticationSessionResult> RegisterAsync(
            RegisterCustomerRequest request ,
            string? ipAddress ,
            CancellationToken cancellationToken = default )
        {
            return Task.FromResult(
                _sessionResult);
        }

        public Task<AuthenticationSessionResult> LoginAsync(
            LoginCustomerRequest request ,
            string? ipAddress ,
            CancellationToken cancellationToken = default )
        {
            return Task.FromResult(
                _sessionResult);
        }

        public Task<AuthenticationSessionResult> RefreshAsync(
            string? refreshToken ,
            string? ipAddress ,
            CancellationToken cancellationToken = default )
        {
            return Task.FromResult(
                _sessionResult);
        }

        public Task LogoutAsync(
            string? refreshToken ,
            string? ipAddress ,
            CancellationToken cancellationToken = default )
        {
            return Task.CompletedTask;
        }
    }

    private sealed class DashboardAuthenticationServiceStub
        : IDashboardAuthenticationService
    {
        private readonly DashboardAuthenticationSessionResult
            _sessionResult;

        public DashboardAuthenticationServiceStub(
            DashboardAuthenticationSessionResult sessionResult )
        {
            _sessionResult = sessionResult;
        }

        public Task<DashboardAuthenticationSessionResult> LoginAsync(
            LoginAdminRequest request ,
            string? ipAddress ,
            CancellationToken cancellationToken = default )
        {
            return Task.FromResult(
                _sessionResult);
        }

        public Task<DashboardAuthenticationSessionResult> RefreshAsync(
            string? refreshToken ,
            string? ipAddress ,
            CancellationToken cancellationToken = default )
        {
            return Task.FromResult(
                _sessionResult);
        }

        public Task LogoutAsync(
            string? refreshToken ,
            string? ipAddress ,
            CancellationToken cancellationToken = default )
        {
            return Task.CompletedTask;
        }

        public Task<DashboardAuthenticationOperationResult>
            ChangePasswordAsync(
                int userId ,
                ChangeDashboardPasswordRequest request ,
                string? ipAddress ,
                CancellationToken cancellationToken = default )
        {
            throw new NotSupportedException();
        }
    }
}