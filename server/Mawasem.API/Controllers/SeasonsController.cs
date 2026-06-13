using Mawasem.Modules.Season.Application.Commands.CreateSeason;
using Mawasem.Modules.Season.Domain.Interfaces;
using Mawasem.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mawasem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeasonsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ISeasonRepository _repository;

    public SeasonsController( IMediator mediator , ISeasonRepository repository )
    {
        _mediator = mediator;
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSeason( [FromBody] CreateSeasonRequest request )
    {
        var command = new CreateSeasonCommand(
            request.Name ,
            request.Type ,
            request.StartDate ,
            request.EndDate ,
            request.Description ,
            request.BannerImageUrl
        );

        var result = await _mediator.Send(command);

        if ( !result.IsSuccess )
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSeasons()
    {
        var seasons = await _repository.GetAllAsync();
        return Ok(seasons);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveSeason()
    {
        var season = await _repository.GetActiveSeasonAsync();

        if ( season is null )
            return NotFound("No active season found");

        return Ok(season);
    }

    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateSeason( Guid id )
    {
        var season = await _repository.GetByIdAsync(id);

        if ( season is null )
            return NotFound("Season not found");

        season.Activate();
        await _repository.UpdateAsync(season);
        await _repository.SaveChangesAsync();

        return Ok("Season activated successfully");
    }
}

public record CreateSeasonRequest(
    string Name ,
    SeasonType Type ,
    DateTime StartDate ,
    DateTime EndDate ,
    string? Description ,
    string? BannerImageUrl
);