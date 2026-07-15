namespace Mawasem.Application.Features.Customers.Contracts.Requests;

public sealed record BlockCustomerRequest
{
    public string Reason { get; init; } = string.Empty;
}