namespace Mawasem.Application.Features.Products.Contracts.Requests;

public sealed record UploadProductImageRequest
{
    public int? ColorOptionValueId { get; init; }

    public bool IsPrimary { get; init; }

    public Stream Content { get; init; } =
        Stream.Null;

    public string FileName { get; init; } =
        string.Empty;

    public string ContentType { get; init; } =
        string.Empty;

    public long Length { get; init; }
}