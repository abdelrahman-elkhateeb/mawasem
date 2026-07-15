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
}