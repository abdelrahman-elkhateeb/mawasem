namespace Mawasem.Application.Features.Customers.Contracts.Responses;

public sealed record CustomerDetailsResponse
{
    public int Id { get; init; }

    public string FullNameAr { get; init; } = string.Empty;

    public string FullNameEn { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string? Email { get; init; }

    public DateOnly? BirthDate { get; init; }

    public string? Gender { get; init; }

    public string? ReferralSource { get; init; }

    public bool IsBlocked { get; init; }

    public DateTime? BlockedAt { get; init; }

    public string? BlockedReason { get; init; }

    public int TotalOrders { get; init; }

    public int DeliveredOrders { get; init; }

    public decimal TotalSpent { get; init; }

    public int SavedAddressCount { get; init; }

    public int ReviewCount { get; init; }
}