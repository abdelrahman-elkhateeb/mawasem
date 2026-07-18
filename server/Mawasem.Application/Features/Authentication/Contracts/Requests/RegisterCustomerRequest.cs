using Mawasem.Domain.Enums;

namespace Mawasem.Application.Features.Authentication.Contracts.Requests;

public sealed record RegisterCustomerRequest
{
    public string FullNameAr { get; init; } = string.Empty;

    public string FullNameEn { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public DateOnly? BirthDate { get; init; }

    public Gender Gender { get; init; }

    public ReferralSource? ReferralSource { get; init; }

    public string Password { get; init; } = string.Empty;

    public string ConfirmPassword { get; init; } = string.Empty;
}