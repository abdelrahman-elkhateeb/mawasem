using Mawasem.API.Extensions;
using Mawasem.Application.Features.Carts.Contracts.Requests;
using Mawasem.Application.Features.Carts.Contracts.Responses;
using Mawasem.Application.Features.Carts.Interfaces;
using Mawasem.Application.Features.Carts.Models;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/carts")]
[Authorize(Roles = SystemRoles.Customer)]
public sealed class CartsController : ControllerBase
{
    private const string GuestTokenHeaderName =
        "X-Guest-Cart-Token";

    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost("customer")]
    [ProducesResponseType(
        typeof(CartCreationResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(CartCreationResponse),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<CartCreationResponse>>
        GetOrCreateForCustomerAsync(
            CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return InvalidAuthenticationToken();
        }

        var result = await _cartService
            .GetOrCreateForCustomerAsync(
                userId,
                cancellationToken);

        if (!result.Succeeded)
        {
            return CreateFailureResponse(
                result.ErrorCode,
                result.ErrorMessage);
        }

        if (result.Response!.WasCreated)
        {
            return StatusCode(
                StatusCodes.Status201Created,
                result.Response);
        }

        return Ok(result.Response);
    }

    [HttpGet("customer")]
    [ProducesResponseType(
        typeof(CartDetailsResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDetailsResponse>>
        GetForCustomerAsync(
            CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return InvalidAuthenticationToken();
        }

        var result = await _cartService.GetForCustomerAsync(
            userId,
            cancellationToken);

        if (!result.Succeeded)
        {
            return CreateFailureResponse(
                result.ErrorCode,
                result.ErrorMessage);
        }

        return Ok(result.Response);
    }

    [AllowAnonymous]
    [HttpPost("guest")]
    [ProducesResponseType(
        typeof(GuestCartCreationResponse),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<GuestCartCreationResponse>>
        CreateGuestAsync(
            CancellationToken cancellationToken)
    {
        var result = await _cartService.CreateGuestAsync(
            cancellationToken);

        if (!result.Succeeded)
        {
            return CreateFailureResponse(
                result.ErrorCode,
                result.ErrorMessage);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            result.Response);
    }

    [AllowAnonymous]
    [HttpGet("guest")]
    [ProducesResponseType(
        typeof(CartDetailsResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDetailsResponse>>
        GetForGuestAsync(
            [FromHeader(Name = GuestTokenHeaderName)]
            string? guestToken,
            CancellationToken cancellationToken)
    {
        var result = await _cartService.GetForGuestAsync(
            guestToken ?? string.Empty,
            cancellationToken);

        if (!result.Succeeded)
        {
            return CreateFailureResponse(
                result.ErrorCode,
                result.ErrorMessage);
        }

        return Ok(result.Response);
    }

    [HttpPost("customer/merge-guest")]
    [ProducesResponseType(
        typeof(CartMergeResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<CartMergeResponse>>
        MergeGuestIntoCustomerAsync(
            [FromBody] MergeGuestCartRequest request,
            CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return InvalidAuthenticationToken();
        }

        var result = await _cartService
            .MergeGuestIntoCustomerAsync(
                userId,
                request.Token,
                cancellationToken);

        if (!result.Succeeded)
        {
            return CreateFailureResponse(
                result.ErrorCode,
                result.ErrorMessage);
        }

        return Ok(result.Response);
    }

    [HttpPost("customer/items")]
    [ProducesResponseType(
        typeof(AddCartItemResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(AddCartItemResponse),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<AddCartItemResponse>>
        AddForCustomerAsync(
            [FromBody] AddCartItemRequest request,
            CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return InvalidAuthenticationToken();
        }

        var result = await _cartService.AddForCustomerAsync(
            userId,
            request.ProductVariantId,
            request.Quantity,
            cancellationToken);

        if (!result.Succeeded)
        {
            return CreateFailureResponse(
                result.ErrorCode,
                result.ErrorMessage);
        }

        if (result.Response!.WasCreated)
        {
            return StatusCode(
                StatusCodes.Status201Created,
                result.Response);
        }

        return Ok(result.Response);
    }

    [AllowAnonymous]
    [HttpPost("guest/items")]
    [ProducesResponseType(
        typeof(AddCartItemResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(AddCartItemResponse),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<AddCartItemResponse>>
        AddForGuestAsync(
            [FromBody] AddGuestCartItemRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _cartService.AddForGuestAsync(
            request.Token,
            request.ProductVariantId,
            request.Quantity,
            cancellationToken);

        if (!result.Succeeded)
        {
            return CreateFailureResponse(
                result.ErrorCode,
                result.ErrorMessage);
        }

        if (result.Response!.WasCreated)
        {
            return StatusCode(
                StatusCodes.Status201Created,
                result.Response);
        }

        return Ok(result.Response);
    }

    [HttpPut("customer/items/{cartItemId:int}")]
    [ProducesResponseType(
        typeof(UpdateCartItemResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateCartItemResponse>>
        UpdateForCustomerAsync(
            int cartItemId,
            [FromBody] UpdateCartItemQuantityRequest request,
            CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return InvalidAuthenticationToken();
        }

        var result = await _cartService.UpdateForCustomerAsync(
            userId,
            cartItemId,
            request.Quantity,
            cancellationToken);

        if (!result.Succeeded)
        {
            return CreateFailureResponse(
                result.ErrorCode,
                result.ErrorMessage);
        }

        return Ok(result.Response);
    }

    [AllowAnonymous]
    [HttpPut("guest/items/{cartItemId:int}")]
    [ProducesResponseType(
        typeof(UpdateCartItemResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateCartItemResponse>>
        UpdateForGuestAsync(
            int cartItemId,
            [FromBody] UpdateGuestCartItemQuantityRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _cartService.UpdateForGuestAsync(
            request.Token,
            cartItemId,
            request.Quantity,
            cancellationToken);

        if (!result.Succeeded)
        {
            return CreateFailureResponse(
                result.ErrorCode,
                result.ErrorMessage);
        }

        return Ok(result.Response);
    }

    [HttpDelete("customer/items/{cartItemId:int}")]
    [ProducesResponseType(
        typeof(CartMutationResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<CartMutationResponse>>
        RemoveForCustomerAsync(
            int cartItemId,
            CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return InvalidAuthenticationToken();
        }

        var result = await _cartService.RemoveForCustomerAsync(
            userId,
            cartItemId,
            cancellationToken);

        if (!result.Succeeded)
        {
            return CreateFailureResponse(
                result.ErrorCode,
                result.ErrorMessage);
        }

        return Ok(result.Response);
    }

    [AllowAnonymous]
    [HttpDelete("guest/items/{cartItemId:int}")]
    [ProducesResponseType(
        typeof(CartMutationResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<CartMutationResponse>>
        RemoveForGuestAsync(
            int cartItemId,
            [FromHeader(Name = GuestTokenHeaderName)]
            string? guestToken,
            CancellationToken cancellationToken)
    {
        var result = await _cartService.RemoveForGuestAsync(
            guestToken ?? string.Empty,
            cartItemId,
            cancellationToken);

        if (!result.Succeeded)
        {
            return CreateFailureResponse(
                result.ErrorCode,
                result.ErrorMessage);
        }

        return Ok(result.Response);
    }

    [HttpDelete("customer/items")]
    [ProducesResponseType(
        typeof(CartMutationResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<CartMutationResponse>>
        ClearForCustomerAsync(
            CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return InvalidAuthenticationToken();
        }

        var result = await _cartService.ClearForCustomerAsync(
            userId,
            cancellationToken);

        if (!result.Succeeded)
        {
            return CreateFailureResponse(
                result.ErrorCode,
                result.ErrorMessage);
        }

        return Ok(result.Response);
    }

    [AllowAnonymous]
    [HttpDelete("guest/items")]
    [ProducesResponseType(
        typeof(CartMutationResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<CartMutationResponse>>
        ClearForGuestAsync(
            [FromHeader(Name = GuestTokenHeaderName)]
            string? guestToken,
            CancellationToken cancellationToken)
    {
        var result = await _cartService.ClearForGuestAsync(
            guestToken ?? string.Empty,
            cancellationToken);

        if (!result.Succeeded)
        {
            return CreateFailureResponse(
                result.ErrorCode,
                result.ErrorMessage);
        }

        return Ok(result.Response);
    }

    private ObjectResult InvalidAuthenticationToken()
    {
        return Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Invalid authentication token.",
            detail:
                "The authenticated customer identifier is invalid.");
    }

    private ObjectResult CreateFailureResponse(
        string? errorCode,
        string? errorMessage)
    {
        return errorCode switch
        {
            CartErrorCodes.InvalidCustomer => Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid customer account.",
                detail: errorMessage),

            CartErrorCodes.AccountBlocked => Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Customer account blocked.",
                detail: errorMessage),

            CartErrorCodes.InvalidGuestToken => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid guest cart token.",
                detail: errorMessage),

            CartErrorCodes.GuestCartNotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Guest cart not found.",
                detail: errorMessage),

            CartErrorCodes.GuestCartExpired => Problem(
                statusCode: StatusCodes.Status410Gone,
                title: "Guest cart expired.",
                detail: errorMessage),

            CartErrorCodes.InvalidProductVariant => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid product variant.",
                detail: errorMessage),

            CartErrorCodes.InvalidQuantity => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid cart quantity.",
                detail: errorMessage),

            CartErrorCodes.ProductVariantNotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Product variant not found.",
                detail: errorMessage),

            CartErrorCodes.CartItemNotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Cart item not found.",
                detail: errorMessage),

            CartErrorCodes.ProductUnavailable => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Product unavailable.",
                detail: errorMessage),

            CartErrorCodes.ProductVariantUnavailable => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Product variant unavailable.",
                detail: errorMessage),

            CartErrorCodes.InsufficientStock => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Insufficient stock.",
                detail: errorMessage),

            _ => Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Cart operation failed.",
                detail:
                    "The cart operation could not be completed.")
        };
    }
}
