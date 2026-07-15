using Mawasem.Application.Features.Employees.Contracts.Requests;
using Mawasem.Application.Features.Employees.Contracts.Responses;
using Mawasem.Application.Features.Employees.Models;
using Mawasem.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Employees;

public sealed partial class EmployeeManagementService
{
    public async Task<EmployeeManagementResult<EmployeeListResponse>>
        GetListAsync(
            GetEmployeesRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( request.PageNumber <= 0 )
        {
            return EmployeeManagementResult<EmployeeListResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidRequest ,
                    "Page number must be greater than zero.");
        }

        if ( request.PageSize <= 0 ||
            request.PageSize > MaximumPageSize )
        {
            return EmployeeManagementResult<EmployeeListResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidRequest ,
                    $"Page size must be between 1 and {MaximumPageSize}.");
        }

        var skipCount =
            (long)( request.PageNumber - 1 ) *
            request.PageSize;

        if ( skipCount > int.MaxValue )
        {
            return EmployeeManagementResult<EmployeeListResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidRequest ,
                    "The requested page is outside the supported range.");
        }

        var search =
            request.Search?.Trim();

        if ( search?.Length > MaximumSearchLength )
        {
            return EmployeeManagementResult<EmployeeListResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidRequest ,
                    $"Search text cannot exceed {MaximumSearchLength} characters.");
        }

        var dashboardRoleNames =
            SystemRoles.DashboardRoles.ToArray();

        var dashboardUserIds =
            from userRole
                in _dbContext.UserRoles.AsNoTracking()
            join role
                in _dbContext.Roles.AsNoTracking()
                on userRole.RoleId equals role.Id
            where
                role.Name != null &&
                dashboardRoleNames.Contains(role.Name)
            select userRole.UserId;

        var employeeQuery =
            _dbContext.Users
                .AsNoTracking()
                .Where(user =>
                    dashboardUserIds.Contains(user.Id));

        if ( !string.IsNullOrWhiteSpace(search) )
        {
            employeeQuery =
                employeeQuery.Where(user =>
                    user.FullNameAr.Contains(search) ||
                    user.FullNameEn.Contains(search) ||
                    ( user.Email != null &&
                     user.Email.Contains(search) ));
        }

        if ( request.IsBlocked.HasValue )
        {
            employeeQuery =
                employeeQuery.Where(user =>
                    user.IsBlocked ==
                    request.IsBlocked.Value);
        }

        var totalCount =
            await employeeQuery
                .CountAsync(cancellationToken);

        var employees =
            await employeeQuery
                .OrderBy(user =>
                    user.FullNameEn)
                .ThenBy(user =>
                    user.Id)
                .Skip((int)skipCount)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

        var employeeResponses =
            await BuildEmployeeResponsesAsync(
                employees ,
                cancellationToken);

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)request.PageSize);

        var response =
            new EmployeeListResponse
            {
                Items = employeeResponses ,
                PageNumber = request.PageNumber ,
                PageSize = request.PageSize ,
                TotalCount = totalCount ,
                TotalPages = totalPages
            };

        return EmployeeManagementResult<EmployeeListResponse>
            .Success(response);
    }

    public async Task<EmployeeManagementResult<EmployeeResponse>>
        GetByIdAsync(
            int employeeId ,
            CancellationToken cancellationToken = default )
    {
        if ( employeeId <= 0 )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.NotFound ,
                    "The dashboard employee was not found.");
        }

        var employee =
            await _dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    user =>
                        user.Id == employeeId ,
                    cancellationToken);

        if ( employee is null )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.NotFound ,
                    "The dashboard employee was not found.");
        }

        var responses =
            await BuildEmployeeResponsesAsync(
                new[] { employee } ,
                cancellationToken);

        var response =
            responses.Single();

        if ( response.Roles.Count == 0 )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.NotFound ,
                    "The dashboard employee was not found.");
        }

        return EmployeeManagementResult<EmployeeResponse>
            .Success(response);
    }

    public async Task<
        EmployeeManagementResult<EmployeeAccessOptionsResponse>>
        GetAccessOptionsAsync(
            int actorUserId ,
            CancellationToken cancellationToken = default )
    {
        var actorAccess =
            await GetActorAccessAsync(
                actorUserId ,
                cancellationToken);

        if ( actorAccess is null )
        {
            return EmployeeManagementResult<
                EmployeeAccessOptionsResponse>.Failure(
                    EmployeeManagementErrorCodes.Forbidden ,
                    "The authenticated account cannot manage dashboard employees.");
        }

        var roleNames =
            GetAssignableRoleNames(
                actorAccess);

        var permissionNames =
            await GetAssignablePermissionNamesAsync(
                actorAccess ,
                cancellationToken);

        var response =
            new EmployeeAccessOptionsResponse
            {
                RoleNames = roleNames ,
                PermissionNames =
                    permissionNames
            };

        return EmployeeManagementResult<
            EmployeeAccessOptionsResponse>.Success(
                response);
    }
}