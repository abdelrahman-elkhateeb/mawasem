using Mawasem.Infrastructure.Storage.Images;
using Microsoft.Extensions.Options;

namespace Mawasem.Tests.Products;

public sealed class LocalProductImageStorageTests
{
    [Fact]
    public async Task SaveAsync_SavesSupportedImagesWhenSignaturesMatch()
    {
        var testCases =
            new[]
            {
                new ImageTestCase(
                    "photo.jpeg" ,
                    " IMAGE/JPEG; charset=binary " ,
                    ".jpg" ,
                    CreateJpegBytes()) ,

                new ImageTestCase(
                    "photo.png" ,
                    "image/png" ,
                    ".png" ,
                    CreatePngBytes()) ,

                new ImageTestCase(
                    "photo.webp" ,
                    "image/webp" ,
                    ".webp" ,
                    CreateWebPBytes())
            };

        foreach ( var testCase in testCases )
        {
            var rootPath =
                CreateTemporaryRootPath();

            try
            {
                var storage =
                    CreateStorage(rootPath);

                await using var content =
                    new MemoryStream(
                        testCase.Content);

                var result =
                    await storage.SaveAsync(
                        17 ,
                        content ,
                        testCase.FileName ,
                        testCase.ContentType);

                Assert.StartsWith(
                    "17/" ,
                    result.StorageKey);

                Assert.EndsWith(
                    testCase.ExpectedExtension ,
                    result.StorageKey);

                Assert.Equal(
                    $"/uploads/products/{result.StorageKey}" ,
                    result.ImageUrl);

                var storedFilePath =
                    ResolveStoredFilePath(
                        rootPath ,
                        result.StorageKey);

                Assert.True(
                    File.Exists(storedFilePath));

                Assert.Equal(
                    testCase.Content ,
                    await File.ReadAllBytesAsync(
                        storedFilePath));
            }
            finally
            {
                DeleteTemporaryRootPath(
                    rootPath);
            }
        }
    }

    [Fact]
    public async Task SaveAsync_RejectsMismatchedAndIncompleteSignaturesWithoutWritingFiles()
    {
        var testCases =
            new[]
            {
                new InvalidImageTestCase(
                    "image/jpeg" ,
                    CreatePngBytes()) ,

                new InvalidImageTestCase(
                    "image/png" ,
                    CreateWebPBytes()) ,

                new InvalidImageTestCase(
                    "image/webp" ,
                    CreateJpegBytes()) ,

                new InvalidImageTestCase(
                    "image/jpeg" ,
                    new byte[]
                    {
                        0xFF,
                        0xD8
                    }) ,

                new InvalidImageTestCase(
                    "image/png" ,
                    Array.Empty<byte>())
            };

        foreach ( var testCase in testCases )
        {
            var rootPath =
                CreateTemporaryRootPath();

            try
            {
                var storage =
                    CreateStorage(rootPath);

                await using var content =
                    new MemoryStream(
                        testCase.Content);

                var exception =
                    await Assert.ThrowsAsync<
                        InvalidDataException>(
                        () =>
                            storage.SaveAsync(
                                17 ,
                                content ,
                                "image.bin" ,
                                testCase.ContentType));

                Assert.Equal(
                    "The product image content does not match the declared content type." ,
                    exception.Message);

                Assert.False(
                    Directory.Exists(rootPath));
            }
            finally
            {
                DeleteTemporaryRootPath(
                    rootPath);
            }
        }
    }

    [Fact]
    public async Task SaveAsync_RejectsUnsupportedContentTypeWithoutWritingFile()
    {
        var rootPath =
            CreateTemporaryRootPath();

        try
        {
            var storage =
                CreateStorage(rootPath);

            await using var content =
                new MemoryStream(
                    CreateJpegBytes());

            var exception =
                await Assert.ThrowsAsync<
                    InvalidOperationException>(
                    () =>
                        storage.SaveAsync(
                            17 ,
                            content ,
                            "image.gif" ,
                            "image/gif"));

            Assert.Equal(
                "The product image content type is not supported." ,
                exception.Message);

            Assert.False(
                Directory.Exists(rootPath));
        }
        finally
        {
            DeleteTemporaryRootPath(
                rootPath);
        }
    }

    [Fact]
    public async Task DeleteAsync_DeletesValidFileAndRejectsPathTraversal()
    {
        var rootPath =
            CreateTemporaryRootPath();

        try
        {
            var storage =
                CreateStorage(rootPath);

            await using var content =
                new MemoryStream(
                    CreateJpegBytes());

            var storedImage =
                await storage.SaveAsync(
                    17 ,
                    content ,
                    "image.jpg" ,
                    "image/jpeg");

            var storedFilePath =
                ResolveStoredFilePath(
                    rootPath ,
                    storedImage.StorageKey);

            Assert.True(
                File.Exists(storedFilePath));

            await storage.DeleteAsync(
                storedImage.StorageKey);

            Assert.False(
                File.Exists(storedFilePath));

            var traversalException =
                await Assert.ThrowsAsync<
                    InvalidOperationException>(
                    () =>
                        storage.DeleteAsync(
                            "../outside.jpg"));

            Assert.Equal(
                "The product image storage key is invalid." ,
                traversalException.Message);
        }
        finally
        {
            DeleteTemporaryRootPath(
                rootPath);
        }
    }

    private static LocalProductImageStorage
        CreateStorage(
            string rootPath )
    {
        return new LocalProductImageStorage(
            Options.Create(
                new ProductImageStorageOptions
                {
                    RootPath = rootPath ,
                    RequestPath =
                        "/uploads/products"
                }));
    }

    private static string
        CreateTemporaryRootPath()
    {
        return Path.Combine(
            Path.GetTempPath() ,
            "Mawasem.Tests" ,
            Guid.NewGuid().ToString("N"));
    }

    private static string
        ResolveStoredFilePath(
            string rootPath ,
            string storageKey )
    {
        return Path.Combine(
            rootPath ,
            storageKey.Replace(
                '/' ,
                Path.DirectorySeparatorChar));
    }

    private static void DeleteTemporaryRootPath(
        string rootPath )
    {
        if ( Directory.Exists(rootPath) )
        {
            Directory.Delete(
                rootPath ,
                recursive: true);
        }
    }

    private static byte[] CreateJpegBytes()
    {
        return
        [
            0xFF,
            0xD8,
            0xFF,
            0xE0,
            0x00,
            0x10,
            0x4A,
            0x46,
            0x49,
            0x46,
            0x00,
            0x01,
            0xFF,
            0xD9
        ];
    }

    private static byte[] CreatePngBytes()
    {
        return
        [
            0x89,
            0x50,
            0x4E,
            0x47,
            0x0D,
            0x0A,
            0x1A,
            0x0A,
            0x00,
            0x00,
            0x00,
            0x0D
        ];
    }

    private static byte[] CreateWebPBytes()
    {
        return
        [
            (byte)'R',
            (byte)'I',
            (byte)'F',
            (byte)'F',
            0x04,
            0x00,
            0x00,
            0x00,
            (byte)'W',
            (byte)'E',
            (byte)'B',
            (byte)'P',
            (byte)'V',
            (byte)'P',
            (byte)'8',
            (byte)' '
        ];
    }

    private sealed record ImageTestCase(
        string FileName ,
        string ContentType ,
        string ExpectedExtension ,
        byte[] Content );

    private sealed record InvalidImageTestCase(
        string ContentType ,
        byte[] Content );
}