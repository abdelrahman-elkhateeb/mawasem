using Mawasem.Application.Features.Products.Models;

namespace Mawasem.Application.Features.Products.Interfaces;

public interface IProductImageStorage
{
    Task<StoredProductImage>
        SaveAsync(
            int productId ,
            Stream content ,
            string fileName ,
            string contentType ,
            CancellationToken cancellationToken = default );

    Task DeleteAsync(
        string storageKey ,
        CancellationToken cancellationToken = default );
}