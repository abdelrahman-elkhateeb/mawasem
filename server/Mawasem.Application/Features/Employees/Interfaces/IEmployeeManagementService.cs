using Mawasem.Application.Features.Employees.Contracts.Requests;
using Mawasem.Application.Features.Employees.Contracts.Responses;
using Mawasem.Application.Features.Employees.Models;

namespace Mawasem.Application.Features.Employees.Interfaces;

public interface IEmployeeManagementService
{
    Task<EmployeeManagementResult<EmployeeListResponse>> GetListAsync(
        GetEmployeesRequest request ,
        CancellationToken cancellationToken = default );

    Task<EmployeeManagementResult<EmployeeResponse>> GetByIdAsync(
        int employeeId ,
        CancellationToken cancellationToken = default );

    Task<EmployeeManagementResult<EmployeeAccessOptionsResponse>>
        GetAccessOptionsAsync(
            int actorUserId ,
            CancellationToken cancellationToken = default );

    Task<EmployeeManagementResult<EmployeeResponse>> CreateAsync(
        int actorUserId ,
        CreateEmployeeRequest request ,
        CancellationToken cancellationToken = default );

    Task<EmployeeManagementResult<EmployeeResponse>> UpdateAsync(
        int actorUserId ,
        int employeeId ,
        UpdateEmployeeRequest request ,
        string? ipAddress ,
        CancellationToken cancellationToken = default );

    Task<EmployeeManagementOperationResult> BlockAsync(
        int actorUserId ,
        int employeeId ,
        BlockEmployeeRequest request ,
        string? ipAddress ,
        CancellationToken cancellationToken = default );

    Task<EmployeeManagementOperationResult> UnblockAsync(
        int actorUserId ,
        int employeeId ,
        CancellationToken cancellationToken = default );

    Task<EmployeeManagementOperationResult> ResetPasswordAsync(
        int actorUserId ,
        int employeeId ,
        ResetEmployeePasswordRequest request ,
        string? ipAddress ,
        CancellationToken cancellationToken = default );

    Task<EmployeeManagementResult<EmployeeResponse>> AssignRolesAsync(
        int actorUserId ,
        int employeeId ,
        AssignEmployeeRolesRequest request ,
        CancellationToken cancellationToken = default );

    Task<EmployeeManagementResult<EmployeeResponse>>
        AssignPermissionsAsync(
            int actorUserId ,
            int employeeId ,
            AssignEmployeePermissionsRequest request ,
            CancellationToken cancellationToken = default );
}