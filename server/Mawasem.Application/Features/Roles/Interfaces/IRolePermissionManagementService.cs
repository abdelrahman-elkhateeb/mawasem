using Mawasem.Application.Features.Roles.Contracts.Requests;
using Mawasem.Application.Features.Roles.Contracts.Responses;
using Mawasem.Application.Features.Roles.Models;

namespace Mawasem.Application.Features.Roles.Interfaces;

public interface IRolePermissionManagementService
{
    Task<RoleManagementResult<RoleListResponse>> GetListAsync(
        int actorUserId ,
        CancellationToken cancellationToken = default );

    Task<RoleManagementResult<RoleResponse>> GetByNameAsync(
        int actorUserId ,
        string roleName ,
        CancellationToken cancellationToken = default );

    Task<RoleManagementResult<RolePermissionOptionsResponse>>
        GetPermissionOptionsAsync(
            int actorUserId ,
            CancellationToken cancellationToken = default );

    Task<RoleManagementResult<RoleResponse>> UpdatePermissionsAsync(
        int actorUserId ,
        string roleName ,
        UpdateRolePermissionsRequest request ,
        CancellationToken cancellationToken = default );
}