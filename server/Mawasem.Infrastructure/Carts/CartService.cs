using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Mawasem.Application.Features.Carts.Contracts.Responses;
using Mawasem.Application.Features.Carts.Interfaces;
using Mawasem.Application.Features.Carts.Models;
using Mawasem.Domain.Carts;
using Mawasem.Domain.Common;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Carts;

public sealed class CartService : ICartService
{
    private const int GuestCartLifetimeDays = 30;

    private const string GuestActor = "guest";

    private readonly MawasemDbContext _dbContext;

    private readonly TimeProvider _timeProvider;

    public CartService(
        MawasemDbContext dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<CartResult<CartCreationResponse>>
        GetOrCreateForCustomerAsync(
            int userId,
            CancellationToken cancellationToken = default)
    {
        var accessFailure = await ValidateCustomerAsync(
            userId,
            cancellationToken);

        if (accessFailure is not null)
        {
            return Failure<CartCreationResponse>(accessFailure);
        }

        var existingCartId = await _dbContext.Carts
            .AsNoTracking()
            .Where(cart =>
                cart.UserId == userId &&
                !cart.IsDeleted)
            .Select(cart => (int?)cart.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (existingCartId.HasValue)
        {
            return CartResult<CartCreationResponse>.Success(
                new CartCreationResponse
                {
                    Id = existingCartId.Value,
                    WasCreated = false
                });
        }

        var now = _timeProvider.GetUtcNow();
        var actor = GetCustomerActor(userId);

        var cart = new Cart
        {
            UserId = userId,
            CreatedOn = now,
            CreatedBy = actor
        };

        _dbContext.Carts.Add(cart);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CartResult<CartCreationResponse>.Success(
            new CartCreationResponse
            {
                Id = cart.Id,
                WasCreated = true
            });
    }

    public async Task<CartResult<GuestCartCreationResponse>>
        CreateGuestAsync(
            CancellationToken cancellationToken = default)
    {
        var token = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32));

        var now = _timeProvider.GetUtcNow();
        var expiresOn = now.AddDays(GuestCartLifetimeDays);

        var cart = new Cart
        {
            GuestTokenHash = HashGuestToken(token),
            GuestExpiresOn = expiresOn,
            CreatedOn = now,
            CreatedBy = GuestActor
        };

        _dbContext.Carts.Add(cart);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CartResult<GuestCartCreationResponse>.Success(
            new GuestCartCreationResponse
            {
                Id = cart.Id,
                Token = token,
                ExpiresOn = expiresOn
            });
    }

    public async Task<CartResult<CartMergeResponse>>
        MergeGuestIntoCustomerAsync(
            int userId,
            string guestToken,
            CancellationToken cancellationToken = default)
    {
        var accessFailure = await ValidateCustomerAsync(
            userId,
            cancellationToken);

        if (accessFailure is not null)
        {
            return Failure<CartMergeResponse>(accessFailure);
        }

        var guestCartResult = await LoadGuestCartAsync(
            guestToken,
            cancellationToken);

        if (!guestCartResult.Succeeded)
        {
            return FailureFrom<CartMergeResponse>(guestCartResult);
        }

        var now = _timeProvider.GetUtcNow();
        var actor = GetCustomerActor(userId);
        var guestCart = guestCartResult.Response!;

        var customerCart = await LoadCustomerCartAsync(
            userId,
            cancellationToken);

        if (customerCart is null)
        {
            customerCart = new Cart
            {
                UserId = userId,
                CreatedOn = now,
                CreatedBy = actor
            };

            _dbContext.Carts.Add(customerCart);
        }

        var guestItems = guestCart.Items
            .Where(item => !item.IsDeleted)
            .ToArray();

        foreach (var guestItem in guestItems)
        {
            var existingItem = customerCart.Items
                .SingleOrDefault(item =>
                    item.ProductVariantId ==
                    guestItem.ProductVariantId);

            if (existingItem is null)
            {
                customerCart.Items.Add(
                    new CartItem
                    {
                        Cart = customerCart,
                        ProductVariantId =
                            guestItem.ProductVariantId,
                        Quantity = guestItem.Quantity,
                        UnitPriceSnapshot =
                            guestItem.UnitPriceSnapshot,
                        CreatedOn = now,
                        CreatedBy = actor
                    });
            }
            else if (existingItem.IsDeleted)
            {
                Restore(existingItem);
                existingItem.Quantity = guestItem.Quantity;
                existingItem.UnitPriceSnapshot =
                    guestItem.UnitPriceSnapshot;
                MarkModified(existingItem, now, actor);
            }
            else
            {
                existingItem.Quantity += guestItem.Quantity;
                MarkModified(existingItem, now, actor);
            }

            MarkDeleted(guestItem, now, actor);
        }

        MarkModified(customerCart, now, actor);
        MarkDeleted(guestCart, now, actor);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CartResult<CartMergeResponse>.Success(
            new CartMergeResponse
            {
                CartId = customerCart.Id,
                MergedItemCount = guestItems.Length
            });
    }

    public async Task<CartResult<CartDetailsResponse>>
        GetForCustomerAsync(
            int userId,
            CancellationToken cancellationToken = default)
    {
        var accessFailure = await ValidateCustomerAsync(
            userId,
            cancellationToken);

        if (accessFailure is not null)
        {
            return Failure<CartDetailsResponse>(accessFailure);
        }

        var cart = await LoadCustomerCartAsync(
            userId,
            cancellationToken);

        if (cart is null)
        {
            var now = _timeProvider.GetUtcNow();

            cart = new Cart
            {
                UserId = userId,
                CreatedOn = now,
                CreatedBy = GetCustomerActor(userId)
            };

            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return CartResult<CartDetailsResponse>.Success(
            CreateDetailsResponse(cart));
    }

    public async Task<CartResult<CartDetailsResponse>>
        GetForGuestAsync(
            string guestToken,
            CancellationToken cancellationToken = default)
    {
        var guestCartResult = await LoadGuestCartAsync(
            guestToken,
            cancellationToken);

        if (!guestCartResult.Succeeded)
        {
            return FailureFrom<CartDetailsResponse>(guestCartResult);
        }

        return CartResult<CartDetailsResponse>.Success(
            CreateDetailsResponse(guestCartResult.Response!));
    }

    public async Task<CartResult<AddCartItemResponse>>
        AddForCustomerAsync(
            int userId,
            int productVariantId,
            int quantity,
            CancellationToken cancellationToken = default)
    {
        var accessFailure = await ValidateCustomerAsync(
            userId,
            cancellationToken);

        if (accessFailure is not null)
        {
            return Failure<AddCartItemResponse>(accessFailure);
        }

        var now = _timeProvider.GetUtcNow();
        var actor = GetCustomerActor(userId);

        var cart = await LoadCustomerCartAsync(
            userId,
            cancellationToken);

        if (cart is null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedOn = now,
                CreatedBy = actor
            };

            _dbContext.Carts.Add(cart);
        }

        return await AddItemAsync(
            cart,
            productVariantId,
            quantity,
            now,
            actor,
            cancellationToken);
    }

    public async Task<CartResult<AddCartItemResponse>>
        AddForGuestAsync(
            string guestToken,
            int productVariantId,
            int quantity,
            CancellationToken cancellationToken = default)
    {
        var guestCartResult = await LoadGuestCartAsync(
            guestToken,
            cancellationToken);

        if (!guestCartResult.Succeeded)
        {
            return FailureFrom<AddCartItemResponse>(guestCartResult);
        }

        return await AddItemAsync(
            guestCartResult.Response!,
            productVariantId,
            quantity,
            _timeProvider.GetUtcNow(),
            GuestActor,
            cancellationToken);
    }

    public async Task<CartResult<UpdateCartItemResponse>>
        UpdateForCustomerAsync(
            int userId,
            int cartItemId,
            int quantity,
            CancellationToken cancellationToken = default)
    {
        var accessFailure = await ValidateCustomerAsync(
            userId,
            cancellationToken);

        if (accessFailure is not null)
        {
            return Failure<UpdateCartItemResponse>(accessFailure);
        }

        var cart = await LoadCustomerCartAsync(
            userId,
            cancellationToken);

        if (cart is null)
        {
            return CartResult<UpdateCartItemResponse>.Failure(
                CartErrorCodes.CartItemNotFound,
                "The cart item was not found.");
        }

        return await UpdateItemAsync(
            cart,
            cartItemId,
            quantity,
            _timeProvider.GetUtcNow(),
            GetCustomerActor(userId),
            cancellationToken);
    }

    public async Task<CartResult<UpdateCartItemResponse>>
        UpdateForGuestAsync(
            string guestToken,
            int cartItemId,
            int quantity,
            CancellationToken cancellationToken = default)
    {
        var guestCartResult = await LoadGuestCartAsync(
            guestToken,
            cancellationToken);

        if (!guestCartResult.Succeeded)
        {
            return FailureFrom<UpdateCartItemResponse>(guestCartResult);
        }

        return await UpdateItemAsync(
            guestCartResult.Response!,
            cartItemId,
            quantity,
            _timeProvider.GetUtcNow(),
            GuestActor,
            cancellationToken);
    }

    public async Task<CartResult<CartMutationResponse>>
        RemoveForCustomerAsync(
            int userId,
            int cartItemId,
            CancellationToken cancellationToken = default)
    {
        var accessFailure = await ValidateCustomerAsync(
            userId,
            cancellationToken);

        if (accessFailure is not null)
        {
            return Failure<CartMutationResponse>(accessFailure);
        }

        var cart = await LoadCustomerCartAsync(
            userId,
            cancellationToken);

        if (cart is null)
        {
            return CartItemNotFound<CartMutationResponse>();
        }

        return await RemoveItemAsync(
            cart,
            cartItemId,
            _timeProvider.GetUtcNow(),
            GetCustomerActor(userId),
            cancellationToken);
    }

    public async Task<CartResult<CartMutationResponse>>
        RemoveForGuestAsync(
            string guestToken,
            int cartItemId,
            CancellationToken cancellationToken = default)
    {
        var guestCartResult = await LoadGuestCartAsync(
            guestToken,
            cancellationToken);

        if (!guestCartResult.Succeeded)
        {
            return FailureFrom<CartMutationResponse>(guestCartResult);
        }

        return await RemoveItemAsync(
            guestCartResult.Response!,
            cartItemId,
            _timeProvider.GetUtcNow(),
            GuestActor,
            cancellationToken);
    }

    public async Task<CartResult<CartMutationResponse>>
        ClearForCustomerAsync(
            int userId,
            CancellationToken cancellationToken = default)
    {
        var accessFailure = await ValidateCustomerAsync(
            userId,
            cancellationToken);

        if (accessFailure is not null)
        {
            return Failure<CartMutationResponse>(accessFailure);
        }

        var cart = await LoadCustomerCartAsync(
            userId,
            cancellationToken);

        if (cart is null)
        {
            var creationResult = await GetOrCreateForCustomerAsync(
                userId,
                cancellationToken);

            return CartResult<CartMutationResponse>.Success(
                new CartMutationResponse
                {
                    CartId = creationResult.Response!.Id,
                    AffectedItemCount = 0
                });
        }

        return await ClearCartAsync(
            cart,
            _timeProvider.GetUtcNow(),
            GetCustomerActor(userId),
            cancellationToken);
    }

    public async Task<CartResult<CartMutationResponse>>
        ClearForGuestAsync(
            string guestToken,
            CancellationToken cancellationToken = default)
    {
        var guestCartResult = await LoadGuestCartAsync(
            guestToken,
            cancellationToken);

        if (!guestCartResult.Succeeded)
        {
            return FailureFrom<CartMutationResponse>(guestCartResult);
        }

        return await ClearCartAsync(
            guestCartResult.Response!,
            _timeProvider.GetUtcNow(),
            GuestActor,
            cancellationToken);
    }

    private async Task<CartResult<AddCartItemResponse>> AddItemAsync(
        Cart cart,
        int productVariantId,
        int quantity,
        DateTimeOffset now,
        string actor,
        CancellationToken cancellationToken)
    {
        if (quantity <= 0)
        {
            return CartResult<AddCartItemResponse>.Failure(
                CartErrorCodes.InvalidQuantity,
                "The quantity must be greater than zero.");
        }

        var existingItem = cart.Items
            .SingleOrDefault(item =>
                item.ProductVariantId == productVariantId);

        var requestedTotalQuantity =
            existingItem is not null && !existingItem.IsDeleted
                ? existingItem.Quantity + quantity
                : quantity;

        var variantResult = await LoadPurchasableVariantAsync(
            productVariantId,
            requestedTotalQuantity,
            cancellationToken);

        if (!variantResult.Succeeded)
        {
            return FailureFrom<AddCartItemResponse>(variantResult);
        }

        var variant = variantResult.Response!;
        var wasCreated = existingItem is null || existingItem.IsDeleted;

        if (existingItem is null)
        {
            existingItem = new CartItem
            {
                Cart = cart,
                ProductVariantId = productVariantId,
                Quantity = quantity,
                UnitPriceSnapshot = variant.Product.CurrentPrice,
                CreatedOn = now,
                CreatedBy = actor
            };

            cart.Items.Add(existingItem);
        }
        else if (existingItem.IsDeleted)
        {
            Restore(existingItem);
            existingItem.Quantity = quantity;
            existingItem.UnitPriceSnapshot =
                variant.Product.CurrentPrice;
            MarkModified(existingItem, now, actor);
        }
        else
        {
            existingItem.Quantity = requestedTotalQuantity;
            existingItem.UnitPriceSnapshot =
                variant.Product.CurrentPrice;
            MarkModified(existingItem, now, actor);
        }

        MarkModified(cart, now, actor);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CartResult<AddCartItemResponse>.Success(
            new AddCartItemResponse
            {
                CartId = cart.Id,
                CartItemId = existingItem.Id,
                ProductVariantId = productVariantId,
                Quantity = existingItem.Quantity,
                UnitPriceSnapshot = existingItem.UnitPriceSnapshot,
                LineTotal = CalculateLineTotal(
                    existingItem.UnitPriceSnapshot,
                    existingItem.Quantity),
                WasCreated = wasCreated
            });
    }

    private async Task<CartResult<UpdateCartItemResponse>> UpdateItemAsync(
        Cart cart,
        int cartItemId,
        int quantity,
        DateTimeOffset now,
        string actor,
        CancellationToken cancellationToken)
    {
        if (quantity <= 0)
        {
            return CartResult<UpdateCartItemResponse>.Failure(
                CartErrorCodes.InvalidQuantity,
                "The quantity must be greater than zero.");
        }

        var cartItem = cart.Items.SingleOrDefault(item =>
            item.Id == cartItemId &&
            !item.IsDeleted);

        if (cartItem is null)
        {
            return CartItemNotFound<UpdateCartItemResponse>();
        }

        var variantResult = await LoadPurchasableVariantAsync(
            cartItem.ProductVariantId,
            quantity,
            cancellationToken);

        if (!variantResult.Succeeded)
        {
            return FailureFrom<UpdateCartItemResponse>(variantResult);
        }

        cartItem.Quantity = quantity;
        cartItem.UnitPriceSnapshot =
            variantResult.Response!.Product.CurrentPrice;

        MarkModified(cartItem, now, actor);
        MarkModified(cart, now, actor);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CartResult<UpdateCartItemResponse>.Success(
            new UpdateCartItemResponse
            {
                CartId = cart.Id,
                CartItemId = cartItem.Id,
                ProductVariantId = cartItem.ProductVariantId,
                Quantity = cartItem.Quantity,
                UnitPriceSnapshot = cartItem.UnitPriceSnapshot,
                LineTotal = CalculateLineTotal(
                    cartItem.UnitPriceSnapshot,
                    cartItem.Quantity)
            });
    }

    private async Task<CartResult<CartMutationResponse>> RemoveItemAsync(
        Cart cart,
        int cartItemId,
        DateTimeOffset now,
        string actor,
        CancellationToken cancellationToken)
    {
        var cartItem = cart.Items.SingleOrDefault(item =>
            item.Id == cartItemId &&
            !item.IsDeleted);

        if (cartItem is null)
        {
            return CartItemNotFound<CartMutationResponse>();
        }

        MarkDeleted(cartItem, now, actor);
        MarkModified(cart, now, actor);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CartResult<CartMutationResponse>.Success(
            new CartMutationResponse
            {
                CartId = cart.Id,
                AffectedItemCount = 1
            });
    }

    private async Task<CartResult<CartMutationResponse>> ClearCartAsync(
        Cart cart,
        DateTimeOffset now,
        string actor,
        CancellationToken cancellationToken)
    {
        var activeItems = cart.Items
            .Where(item => !item.IsDeleted)
            .ToArray();

        foreach (var item in activeItems)
        {
            MarkDeleted(item, now, actor);
        }

        if (activeItems.Length > 0)
        {
            MarkModified(cart, now, actor);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return CartResult<CartMutationResponse>.Success(
            new CartMutationResponse
            {
                CartId = cart.Id,
                AffectedItemCount = activeItems.Length
            });
    }

    private async Task<AccessFailure?> ValidateCustomerAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            return new AccessFailure(
                CartErrorCodes.InvalidCustomer,
                "The authenticated customer account was not found.");
        }

        var customer = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new
            {
                user.IsBlocked
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            return new AccessFailure(
                CartErrorCodes.InvalidCustomer,
                "The authenticated customer account was not found.");
        }

        if (customer.IsBlocked)
        {
            return new AccessFailure(
                CartErrorCodes.AccountBlocked,
                "The authenticated customer account is blocked.");
        }

        return null;
    }

    private async Task<CartResult<Cart>> LoadGuestCartAsync(
        string guestToken,
        CancellationToken cancellationToken)
    {
        if (!IsValidGuestToken(guestToken))
        {
            return CartResult<Cart>.Failure(
                CartErrorCodes.InvalidGuestToken,
                "The guest cart token is invalid.");
        }

        var tokenHash = HashGuestToken(guestToken);

        var cart = await CartQuery()
            .SingleOrDefaultAsync(
                candidate =>
                    candidate.UserId == null &&
                    candidate.GuestTokenHash == tokenHash &&
                    !candidate.IsDeleted,
                cancellationToken);

        if (cart is null)
        {
            return CartResult<Cart>.Failure(
                CartErrorCodes.GuestCartNotFound,
                "The guest cart was not found.");
        }

        if (!cart.GuestExpiresOn.HasValue ||
            cart.GuestExpiresOn.Value <= _timeProvider.GetUtcNow())
        {
            return CartResult<Cart>.Failure(
                CartErrorCodes.GuestCartExpired,
                "The guest cart has expired.");
        }

        return CartResult<Cart>.Success(cart);
    }

    private Task<Cart?> LoadCustomerCartAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        return CartQuery().SingleOrDefaultAsync(
            cart =>
                cart.UserId == userId &&
                !cart.IsDeleted,
            cancellationToken);
    }

    private IQueryable<Cart> CartQuery()
    {
        return _dbContext.Carts
            .Include(cart => cart.Items)
                .ThenInclude(item => item.ProductVariant)
                    .ThenInclude(variant => variant.Product);
    }

    private async Task<CartResult<ProductVariant>>
        LoadPurchasableVariantAsync(
            int productVariantId,
            int requestedQuantity,
            CancellationToken cancellationToken)
    {
        if (productVariantId <= 0)
        {
            return CartResult<ProductVariant>.Failure(
                CartErrorCodes.InvalidProductVariant,
                "The product variant identifier is invalid.");
        }

        var variant = await _dbContext.ProductVariants
            .Include(candidate => candidate.Product)
            .SingleOrDefaultAsync(
                candidate =>
                    candidate.Id == productVariantId &&
                    !candidate.IsDeleted,
                cancellationToken);

        if (variant is null)
        {
            return CartResult<ProductVariant>.Failure(
                CartErrorCodes.ProductVariantNotFound,
                "The product variant was not found.");
        }

        if (variant.Product.IsDeleted ||
            !variant.Product.IsPublished)
        {
            return CartResult<ProductVariant>.Failure(
                CartErrorCodes.ProductUnavailable,
                "The product is not currently available for purchase.");
        }

        if (!variant.IsAvailable)
        {
            return CartResult<ProductVariant>.Failure(
                CartErrorCodes.ProductVariantUnavailable,
                "The product variant is not currently available for purchase.");
        }

        if (variant.StockQuantity < requestedQuantity)
        {
            return CartResult<ProductVariant>.Failure(
                CartErrorCodes.InsufficientStock,
                $"Only {variant.StockQuantity} unit(s) are currently available.");
        }

        return CartResult<ProductVariant>.Success(variant);
    }

    private static CartDetailsResponse CreateDetailsResponse(Cart cart)
    {
        var items = cart.Items
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.Id)
            .Select(CreateItemResponse)
            .ToArray();

        return new CartDetailsResponse
        {
            CartId = cart.Id,
            IsGuest = cart.UserId is null,
            GuestExpiresOn = cart.GuestExpiresOn,
            DistinctItemCount = items.Length,
            TotalQuantity = items.Sum(item => item.Quantity),
            Subtotal = items.Sum(item => item.LineTotal),
            HasWarnings = items.Any(item => item.Warnings.Count > 0),
            Items = items
        };
    }

    private static CartItemResponse CreateItemResponse(CartItem cartItem)
    {
        var variant = cartItem.ProductVariant;
        var product = variant.Product;
        var warnings = new List<CartWarningResponse>();

        if (product.IsDeleted || !product.IsPublished)
        {
            warnings.Add(
                new CartWarningResponse
                {
                    Code = CartWarningCodes.ProductUnavailable,
                    Message =
                        "This product is no longer available for purchase."
                });
        }

        if (variant.IsDeleted || !variant.IsAvailable)
        {
            warnings.Add(
                new CartWarningResponse
                {
                    Code = CartWarningCodes.ProductVariantUnavailable,
                    Message =
                        "This product variant is no longer available for purchase."
                });
        }

        if (variant.StockQuantity <= 0)
        {
            warnings.Add(
                new CartWarningResponse
                {
                    Code = CartWarningCodes.OutOfStock,
                    Message = "This product variant is out of stock."
                });
        }
        else if (cartItem.Quantity > variant.StockQuantity)
        {
            warnings.Add(
                new CartWarningResponse
                {
                    Code = CartWarningCodes.InsufficientStock,
                    Message =
                        $"Only {variant.StockQuantity} unit(s) are currently available."
                });
        }

        if (cartItem.UnitPriceSnapshot != product.CurrentPrice)
        {
            warnings.Add(
                new CartWarningResponse
                {
                    Code = CartWarningCodes.PriceChanged,
                    Message = "The product price has changed."
                });
        }

        var isPurchasable =
            !product.IsDeleted &&
            product.IsPublished &&
            !variant.IsDeleted &&
            variant.IsAvailable &&
            variant.StockQuantity >= cartItem.Quantity;

        return new CartItemResponse
        {
            CartItemId = cartItem.Id,
            ProductVariantId = variant.Id,
            ProductId = product.Id,
            ProductNameEn = product.Name.English,
            ProductNameAr = product.Name.Arabic,
            Sku = variant.SKU,
            OptionCombinationKey = variant.OptionCombinationKey,
            Quantity = cartItem.Quantity,
            UnitPriceSnapshot = cartItem.UnitPriceSnapshot,
            CurrentUnitPrice = product.CurrentPrice,
            LineTotal = CalculateLineTotal(
                product.CurrentPrice,
                cartItem.Quantity),
            StockQuantity = variant.StockQuantity,
            IsPurchasable = isPurchasable,
            Warnings = warnings
        };
    }

    private static bool IsValidGuestToken(string guestToken)
    {
        return !string.IsNullOrWhiteSpace(guestToken) &&
            guestToken.Length == 64 &&
            guestToken.All(Uri.IsHexDigit);
    }

    private static string HashGuestToken(string guestToken)
    {
        return Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(guestToken)));
    }

    private static string GetCustomerActor(int userId)
    {
        return userId.ToString(CultureInfo.InvariantCulture);
    }

    private static decimal CalculateLineTotal(
        decimal unitPrice,
        int quantity)
    {
        return decimal.Round(
            unitPrice * quantity,
            2,
            MidpointRounding.AwayFromZero);
    }

    private static void Restore(BaseAuditableEntity entity)
    {
        entity.IsDeleted = false;
        entity.DeletedOn = null;
        entity.DeletedBy = null;
    }

    private static void MarkModified(
        BaseAuditableEntity entity,
        DateTimeOffset now,
        string actor)
    {
        entity.LastModifiedOn = now;
        entity.LastModifiedBy = actor;
    }

    private static void MarkDeleted(
        BaseAuditableEntity entity,
        DateTimeOffset now,
        string actor)
    {
        entity.IsDeleted = true;
        entity.DeletedOn = now;
        entity.DeletedBy = actor;
        MarkModified(entity, now, actor);
    }

    private static CartResult<TResponse> Failure<TResponse>(
        AccessFailure failure)
    {
        return CartResult<TResponse>.Failure(
            failure.Code,
            failure.Message);
    }

    private static CartResult<TResponse> FailureFrom<TResponse>(
        CartResult<Cart> failure)
    {
        return CartResult<TResponse>.Failure(
            failure.ErrorCode!,
            failure.ErrorMessage!);
    }

    private static CartResult<TResponse> FailureFrom<TResponse>(
        CartResult<ProductVariant> failure)
    {
        return CartResult<TResponse>.Failure(
            failure.ErrorCode!,
            failure.ErrorMessage!);
    }

    private static CartResult<TResponse> CartItemNotFound<TResponse>()
    {
        return CartResult<TResponse>.Failure(
            CartErrorCodes.CartItemNotFound,
            "The cart item was not found.");
    }

    private sealed record AccessFailure(
        string Code,
        string Message);
}
