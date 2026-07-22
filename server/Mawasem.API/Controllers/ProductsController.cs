using Mawasem.Application.Features.PublicCatalog.Contracts.Requests;
using Mawasem.Application.Features.PublicCatalog.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mawasem.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IPublicCatalogService
        _publicCatalogService;

    public ProductsController(
        IPublicCatalogService publicCatalogService )
    {
        ArgumentNullException.ThrowIfNull(
            publicCatalogService);

        _publicCatalogService =
            publicCatalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] GetPublicProductsRequest request ,
        CancellationToken cancellationToken )
    {
        var response =
            await _publicCatalogService
                .GetProductsAsync(
                    request ,
                    cancellationToken);

        return Ok(response);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(
        [FromRoute] string slug ,
        CancellationToken cancellationToken )
    {
        var response =
            await _publicCatalogService
                .GetProductBySlugAsync(
                    slug ,
                    cancellationToken);

        if ( response is null )
        {
            return NotFound();
        }

        return Ok(response);
    }
}
