namespace Mawasem.Application.Features.Authentication.Contracts.Requests;

public sealed record ChangeDashboardPasswordRequest
{
    public string CurrentPassword { get; init; } = string.Empty;

    public string NewPassword { get; init; } = string.Empty;

    public string ConfirmNewPassword { get; init; } = string.Empty;
}