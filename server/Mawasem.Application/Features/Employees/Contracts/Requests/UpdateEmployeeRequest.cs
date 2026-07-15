namespace Mawasem.Application.Features.Employees.Contracts.Requests;

public sealed record UpdateEmployeeRequest
{
    public string FullNameAr { get; init; } = string.Empty;

    public string FullNameEn { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}