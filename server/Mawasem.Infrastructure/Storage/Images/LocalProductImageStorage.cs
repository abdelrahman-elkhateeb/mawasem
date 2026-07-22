using Mawasem.Application.Features.Products.Interfaces;
using Mawasem.Application.Features.Products.Models;
using Microsoft.Extensions.Options;

namespace Mawasem.Infrastructure.Storage.Images;

public sealed class LocalProductImageStorage
    : IProductImageStorage
{
    private const int BufferSize = 81920;
    private const int MaximumSignatureLength = 12;

    private static readonly byte[] PngSignature =
    {
        0x89,
        0x50,
        0x4E,
        0x47,
        0x0D,
        0x0A,
        0x1A,
        0x0A
    };

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

        var normalizedContentType =
            NormalizeContentType(contentType);

        var extension =
            GetExtension(normalizedContentType);

        var signatureBuffer =
            new byte[MaximumSignatureLength];

        var signatureLength =
            await ReadSignatureAsync(
                content ,
                signatureBuffer ,
                cancellationToken);

        if ( !HasExpectedSignature(
                normalizedContentType ,
                signatureBuffer.AsSpan(
                    0 ,
                    signatureLength)) )
        {
            throw new InvalidDataException(
                "The product image content does not match the declared content type.");
        }

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

            await destination.WriteAsync(
                signatureBuffer.AsMemory(
                    0 ,
                    signatureLength) ,
                cancellationToken);

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

    private static async Task<int>
        ReadSignatureAsync(
            Stream content ,
            byte[] signatureBuffer ,
            CancellationToken cancellationToken )
    {
        var totalBytesRead = 0;

        while ( totalBytesRead <
                signatureBuffer.Length )
        {
            var bytesRead =
                await content.ReadAsync(
                    signatureBuffer.AsMemory(
                        totalBytesRead ,
                        signatureBuffer.Length -
                        totalBytesRead) ,
                    cancellationToken);

            if ( bytesRead == 0 )
            {
                break;
            }

            totalBytesRead += bytesRead;
        }

        return totalBytesRead;
    }

    private static bool HasExpectedSignature(
        string contentType ,
        ReadOnlySpan<byte> signature )
    {
        return contentType switch
        {
            "image/jpeg" =>
                signature.Length >= 3 &&
                signature[0] == 0xFF &&
                signature[1] == 0xD8 &&
                signature[2] == 0xFF,

            "image/png" =>
                signature.StartsWith(
                    PngSignature),

            "image/webp" =>
                signature.Length >= 12 &&
                signature[0] == (byte)'R' &&
                signature[1] == (byte)'I' &&
                signature[2] == (byte)'F' &&
                signature[3] == (byte)'F' &&
                signature[8] == (byte)'W' &&
                signature[9] == (byte)'E' &&
                signature[10] == (byte)'B' &&
                signature[11] == (byte)'P',

            _ => false
        };
    }

    private static string NormalizeContentType(
        string contentType )
    {
        return contentType
            .Split(
                ';' ,
                StringSplitOptions.RemoveEmptyEntries)[0]
            .Trim()
            .ToLowerInvariant();
    }

    private static string GetExtension(
        string contentType )
    {
        return contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",

            _ => throw new InvalidOperationException(
                "The product image content type is not supported.")
        };
    }
}