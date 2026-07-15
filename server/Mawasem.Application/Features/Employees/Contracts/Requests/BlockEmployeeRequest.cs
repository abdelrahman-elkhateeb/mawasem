namespace Mawasem.Application.Features.Employees.Contracts.Requests;

public sealed record BlockEmployeeRequest
{
    public string Reason { get; init; } = string.Empty;
}