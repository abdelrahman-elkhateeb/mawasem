namespace Mawasem.Application.Features.Employees.Contracts.Requests;

public sealed record ResetEmployeePasswordRequest
{
    public string TemporaryPassword { get; init; } = string.Empty;

    public string ConfirmTemporaryPassword { get; init; } = string.Empty;
}