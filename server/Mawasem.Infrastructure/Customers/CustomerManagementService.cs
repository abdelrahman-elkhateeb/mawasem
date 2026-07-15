using Mawasem.Application.Features.Customers.Contracts.Requests;
using Mawasem.Application.Features.Customers.Contracts.Responses;
using Mawasem.Application.Features.Customers.Interfaces;
using Mawasem.Application.Features.Customers.Models;
using Mawasem.Domain.Enums;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Customers;

public sealed class CustomerManagementService
    : ICustomerManagementService
{
    private const int MaximumPageSize = 100;

    private const int MaximumSearchLength = 256;

    private readonly MawasemDbContext _dbContext;

    public CustomerManagementService(
        MawasemDbContext dbContext )
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerManagementResult<CustomerListResponse>>
        GetListAsync(
            GetCustomersRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( request.PageNumber <= 0 )
        {
            return CustomerManagementResult<CustomerListResponse>
                .Failure(
                    CustomerManagementErrorCodes.InvalidRequest ,
                    "Page number must be greater than zero.");
        }

        if ( request.PageSize <= 0 ||
            request.PageSize > MaximumPageSize )
        {
            return CustomerManagementResult<CustomerListResponse>
                .Failure(
                    CustomerManagementErrorCodes.InvalidRequest ,
                    $"Page size must be between 1 and {MaximumPageSize}.");
        }

        var skipCount =
            (long)( request.PageNumber - 1 ) *
            request.PageSize;

        if ( skipCount > int.MaxValue )
        {
            return CustomerManagementResult<CustomerListResponse>
                .Failure(
                    CustomerManagementErrorCodes.InvalidRequest ,
                    "The requested page is outside the supported range.");
        }

        var search =
            request.Search?.Trim();

        if ( search?.Length > MaximumSearchLength )
        {
            return CustomerManagementResult<CustomerListResponse>
                .Failure(
                    CustomerManagementErrorCodes.InvalidRequest ,
                    $"Search text cannot exceed {MaximumSearchLength} characters.");
        }

        var customerQuery =
            GetCustomerUsersQuery();

        if ( !string.IsNullOrWhiteSpace(search) )
        {
            customerQuery =
                customerQuery.Where(user =>
                    user.FullNameAr.Contains(search) ||
                    user.FullNameEn.Contains(search) ||
                    ( user.PhoneNumber != null &&
                      user.PhoneNumber.Contains(search) ) ||
                    ( user.Email != null &&
                      user.Email.Contains(search) ));
        }

        if ( request.IsBlocked.HasValue )
        {
            customerQuery =
                customerQuery.Where(user =>
                    user.IsBlocked ==
                    request.IsBlocked.Value);
        }

        var totalCount =
            await customerQuery.CountAsync(
                cancellationToken);

        var customers =
            await customerQuery
                .OrderBy(user =>
                    user.FullNameEn)
                .ThenBy(user =>
                    user.Id)
                .Skip((int)skipCount)
                .Take(request.PageSize)
                .Select(user =>
                    new CustomerListItemResponse
                    {
                        Id = user.Id ,
                        FullNameAr =
                            user.FullNameAr ,
                        FullNameEn =
                            user.FullNameEn ,
                        PhoneNumber =
                            user.PhoneNumber
                            ?? string.Empty ,
                        IsBlocked =
                            user.IsBlocked ,
                        TotalOrders =
                            user.Orders.Count ,
                        TotalSpent =
                            user.Orders
                                .Where(order =>
                                    order.PaymentStatus ==
                                    PaymentStatus.Paid)
                                .Select(order =>
                                    (decimal?)
                                    order.TotalAmount)
                                .Sum()
                            ?? 0m
                    })
                .ToArrayAsync(cancellationToken);

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)request.PageSize);

        var response =
            new CustomerListResponse
            {
                Items = customers ,
                PageNumber = request.PageNumber ,
                PageSize = request.PageSize ,
                TotalCount = totalCount ,
                TotalPages = totalPages
            };

        return CustomerManagementResult<CustomerListResponse>
            .Success(response);
    }

    public async Task<CustomerManagementResult<CustomerDetailsResponse>>
        GetByIdAsync(
            int customerId ,
            CancellationToken cancellationToken = default )
    {
        if ( customerId <= 0 )
        {
            return CustomerManagementResult<CustomerDetailsResponse>
                .Failure(
                    CustomerManagementErrorCodes.NotFound ,
                    "The customer was not found.");
        }

        var customer =
            await GetCustomerUsersQuery()
                .Where(user =>
                    user.Id == customerId)
                .Select(user =>
                    new
                    {
                        user.Id ,
                        user.FullNameAr ,
                        user.FullNameEn ,
                        user.PhoneNumber ,
                        user.Email ,
                        user.BirthDate ,
                        user.Gender ,
                        user.ReferralSource ,
                        user.IsBlocked ,
                        user.BlockedAt ,
                        user.BlockedReason ,

                        TotalOrders =
                            user.Orders.Count ,

                        DeliveredOrders =
                            user.Orders.Count(order =>
                                order.OrderStatus ==
                                OrderStatus.Delivered) ,

                        TotalSpent =
                            user.Orders
                                .Where(order =>
                                    order.PaymentStatus ==
                                    PaymentStatus.Paid)
                                .Select(order =>
                                    (decimal?)
                                    order.TotalAmount)
                                .Sum()
                            ?? 0m ,

                        SavedAddressCount =
                            user.Addresses.Count ,

                        ReviewCount =
                            user.Reviews.Count
                    })
                .SingleOrDefaultAsync(
                    cancellationToken);

        if ( customer is null )
        {
            return CustomerManagementResult<CustomerDetailsResponse>
                .Failure(
                    CustomerManagementErrorCodes.NotFound ,
                    "The customer was not found.");
        }

        var response =
            new CustomerDetailsResponse
            {
                Id = customer.Id ,
                FullNameAr =
                    customer.FullNameAr ,
                FullNameEn =
                    customer.FullNameEn ,
                PhoneNumber =
                    customer.PhoneNumber
                    ?? string.Empty ,
                Email =
                    customer.Email ,
                BirthDate =
                    customer.BirthDate ,
                Gender =
                    customer.Gender?.ToString() ,
                ReferralSource =
                    customer.ReferralSource?.ToString() ,
                IsBlocked =
                    customer.IsBlocked ,
                BlockedAt =
                    customer.BlockedAt ,
                BlockedReason =
                    customer.BlockedReason ,
                TotalOrders =
                    customer.TotalOrders ,
                DeliveredOrders =
                    customer.DeliveredOrders ,
                TotalSpent =
                    customer.TotalSpent ,
                SavedAddressCount =
                    customer.SavedAddressCount ,
                ReviewCount =
                    customer.ReviewCount
            };

        return CustomerManagementResult<CustomerDetailsResponse>
            .Success(response);
    }

    private IQueryable<ApplicationUser> GetCustomerUsersQuery()
    {
        var customerUserIds =
            from userRole
                in _dbContext.UserRoles.AsNoTracking()
            join role
                in _dbContext.Roles.AsNoTracking()
                on userRole.RoleId equals role.Id
            where
                role.Name == SystemRoles.Customer
            select userRole.UserId;

        return _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                customerUserIds.Contains(user.Id));
    }
}