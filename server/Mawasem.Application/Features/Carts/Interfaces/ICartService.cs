using Mawasem.Application.Features.Carts.Contracts.Responses;
using Mawasem.Application.Features.Carts.Models;

namespace Mawasem.Application.Features.Carts.Interfaces;

public interface ICartService
{
    Task<CartResult<CartCreationResponse>>
        GetOrCreateForCustomerAsync(
            int userId,
            CancellationToken cancellationToken = default);

    Task<CartResult<GuestCartCreationResponse>>
        CreateGuestAsync(
            CancellationToken cancellationToken = default);

    Task<CartResult<CartMergeResponse>>
        MergeGuestIntoCustomerAsync(
            int userId,
            string guestToken,
            CancellationToken cancellationToken = default);

    Task<CartResult<CartDetailsResponse>>
        GetForCustomerAsync(
            int userId,
            CancellationToken cancellationToken = default);

    Task<CartResult<CartDetailsResponse>>
        GetForGuestAsync(
            string guestToken,
            CancellationToken cancellationToken = default);

    Task<CartResult<AddCartItemResponse>>
        AddForCustomerAsync(
            int userId,
            int productVariantId,
            int quantity,
            CancellationToken cancellationToken = default);

    Task<CartResult<AddCartItemResponse>>
        AddForGuestAsync(
            string guestToken,
            int productVariantId,
            int quantity,
            CancellationToken cancellationToken = default);

    Task<CartResult<UpdateCartItemResponse>>
        UpdateForCustomerAsync(
            int userId,
            int cartItemId,
            int quantity,
            CancellationToken cancellationToken = default);

    Task<CartResult<UpdateCartItemResponse>>
        UpdateForGuestAsync(
            string guestToken,
            int cartItemId,
            int quantity,
            CancellationToken cancellationToken = default);

    Task<CartResult<CartMutationResponse>>
        RemoveForCustomerAsync(
            int userId,
            int cartItemId,
            CancellationToken cancellationToken = default);

    Task<CartResult<CartMutationResponse>>
        RemoveForGuestAsync(
            string guestToken,
            int cartItemId,
            CancellationToken cancellationToken = default);

    Task<CartResult<CartMutationResponse>>
        ClearForCustomerAsync(
            int userId,
            CancellationToken cancellationToken = default);

    Task<CartResult<CartMutationResponse>>
        ClearForGuestAsync(
            string guestToken,
            CancellationToken cancellationToken = default);
}
