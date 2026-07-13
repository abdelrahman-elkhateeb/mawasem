using Mawasem.Application.Features.Authentication.Contracts.Requests;
using Mawasem.Application.Features.Authentication.Models;

namespace Mawasem.Application.Features.Authentication.Interfaces;

public interface ICustomerPasswordResetService
{
    Task<CustomerPasswordResetOperationResult> RequestCodeAsync(
        ForgotCustomerPasswordRequest request ,
        string? ipAddress ,
        CancellationToken cancellationToken = default );

    Task<CustomerPasswordResetVerificationResult> VerifyCodeAsync(
        VerifyCustomerPasswordResetCodeRequest request ,
        CancellationToken cancellationToken = default );

    Task<CustomerPasswordResetOperationResult> ResetPasswordAsync(
        ResetCustomerPasswordRequest request ,
        string? ipAddress ,
        CancellationToken cancellationToken = default );
}