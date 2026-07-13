namespace Mawasem.Application.Features.Authentication.Models;

public sealed record AccessTokenResult(
    string Token ,
    DateTime ExpiresAtUtc );