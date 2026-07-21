namespace Mawasem.Infrastructure.Storage.Images;

public sealed class ProductImageStorageOptions
{
    public string RootPath { get; set; } =
        string.Empty;

    public string RequestPath { get; set; } =
        "/uploads/products";
}