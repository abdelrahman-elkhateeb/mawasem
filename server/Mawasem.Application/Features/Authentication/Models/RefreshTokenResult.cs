namespace Mawasem.Application.Features.Authentication.Models;

public sealed record RefreshTokenResult(
    string Token ,
    string TokenHash ,
    DateTime ExpiresAtUtc );