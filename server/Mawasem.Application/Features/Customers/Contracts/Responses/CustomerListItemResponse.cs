namespace Mawasem.Application.Features.Customers.Contracts.Responses;

public sealed record CustomerListItemResponse
{
    public int Id { get; init; }

    public string FullNameAr { get; init; } = string.Empty;

    public string FullNameEn { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public bool IsBlocked { get; init; }

    public int TotalOrders { get; init; }

    public decimal TotalSpent { get; init; }
}