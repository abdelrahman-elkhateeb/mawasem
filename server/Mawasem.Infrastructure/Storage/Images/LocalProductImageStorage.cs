using Mawasem.Application.Features.Products.Interfaces;
using Mawasem.Application.Features.Products.Models;
using Microsoft.Extensions.Options;

namespace Mawasem.Infrastructure.Storage.Images;

public sealed class LocalProductImageStorage
    : IProductImageStorage
{
    private const int BufferSize = 81920;

    private readonly string _rootPath;
    private readonly string _requestPath;

    public LocalProductImageStorage(
        IOptions<ProductImageStorageOptions> options )
    {
        ArgumentNullException.ThrowIfNull(options);

        if ( string.IsNullOrWhiteSpace(
                options.Value.RootPath) )
        {
            throw new InvalidOperationException(
                "The product image storage root path is not configured.");
        }

        if ( string.IsNullOrWhiteSpace(
                options.Value.RequestPath) )
        {
            throw new InvalidOperationException(
                "The product image request path is not configured.");
        }

        _rootPath =
            Path.GetFullPath(
                options.Value.RootPath);

        _requestPath =
            $"/{options.Value.RequestPath.Trim().Trim('/')}";
    }

    public async Task<StoredProductImage>
        SaveAsync(
            int productId ,
            Stream content ,
            string fileName ,
            string contentType ,
            CancellationToken cancellationToken = default )
    {
        if ( productId <= 0 )
        {
            throw new ArgumentOutOfRangeException(
                nameof(productId));
        }

        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            contentType);

        if ( !content.CanRead )
        {
            throw new ArgumentException(
                "The image content stream must be readable." ,
                nameof(content));
        }

        var extension =
            GetExtension(contentType);

        var storageKey =
            $"{productId}/{Guid.NewGuid():N}{extension}";

        var filePath =
            ResolveStoragePath(storageKey);

        var directoryPath =
            Path.GetDirectoryName(filePath)!;

        Directory.CreateDirectory(
            directoryPath);

        try
        {
            await using var destination =
                new FileStream(
                    filePath ,
                    FileMode.CreateNew ,
                    FileAccess.Write ,
                    FileShare.None ,
                    BufferSize ,
                    FileOptions.Asynchronous);

            await content.CopyToAsync(
                destination ,
                cancellationToken);
        }
        catch
        {
            if ( File.Exists(filePath) )
            {
                File.Delete(filePath);
            }

            throw;
        }

        return new StoredProductImage(
            storageKey ,
            $"{_requestPath}/{storageKey}");
    }

    public Task DeleteAsync(
        string storageKey ,
        CancellationToken cancellationToken = default )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            storageKey);

        cancellationToken.ThrowIfCancellationRequested();

        var filePath =
            ResolveStoragePath(storageKey);

        if ( File.Exists(filePath) )
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private string ResolveStoragePath(
        string storageKey )
    {
        var normalizedStorageKey =
            storageKey.Replace(
                '/' ,
                Path.DirectorySeparatorChar);

        var filePath =
            Path.GetFullPath(
                Path.Combine(
                    _rootPath ,
                    normalizedStorageKey));

        var rootPathPrefix =
            _rootPath.EndsWith(
                Path.DirectorySeparatorChar)
                ? _rootPath
                : _rootPath +
                  Path.DirectorySeparatorChar;

        var comparison =
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

        if ( !filePath.StartsWith(
                rootPathPrefix ,
                comparison) )
        {
            throw new InvalidOperationException(
                "The product image storage key is invalid.");
        }

        return filePath;
    }

    private static string GetExtension(
        string contentType )
    {
        var normalizedContentType =
            contentType
                .Split(
                    ';' ,
                    StringSplitOptions.RemoveEmptyEntries)[0]
                .Trim()
                .ToLowerInvariant();

        return normalizedContentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",

            _ => throw new InvalidOperationException(
                "The product image content type is not supported.")
        };
    }
}