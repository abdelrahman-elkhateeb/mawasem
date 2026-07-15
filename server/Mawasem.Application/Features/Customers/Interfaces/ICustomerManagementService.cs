using Mawasem.Application.Features.Customers.Contracts.Requests;
using Mawasem.Application.Features.Customers.Contracts.Responses;
using Mawasem.Application.Features.Customers.Models;

namespace Mawasem.Application.Features.Customers.Interfaces;

public interface ICustomerManagementService
{
    Task<CustomerManagementResult<CustomerListResponse>> GetListAsync(
        GetCustomersRequest request ,
        CancellationToken cancellationToken = default );

    Task<CustomerManagementResult<CustomerDetailsResponse>> GetByIdAsync(
        int customerId ,
        CancellationToken cancellationToken = default );

    Task<CustomerManagementOperationResult> BlockAsync(
        int customerId ,
        BlockCustomerRequest request ,
        string? ipAddress ,
        CancellationToken cancellationToken = default );

    Task<CustomerManagementOperationResult> UnblockAsync(
        int customerId ,
        CancellationToken cancellationToken = default );
}