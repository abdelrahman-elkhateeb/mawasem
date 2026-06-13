using Mawasem.Modules.Season.Domain.Entities;
using Mawasem.Modules.Season.Domain.Interfaces;
using Mawasem.Shared.Common;
using MediatR;

namespace Mawasem.Modules.Season.Application.Commands.CreateSeason;

public class CreateSeasonHandler : IRequestHandler<CreateSeasonCommand , Result<Guid>>
{
    private readonly ISeasonRepository _repository;

    public CreateSeasonHandler( ISeasonRepository repository )
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(
        CreateSeasonCommand request ,
        CancellationToken cancellationToken )
    {
        try
        {
            var season = SeasonEntity.Create(
                request.Name ,
                request.Type ,
                request.StartDate ,
                request.EndDate ,
                request.Description ,
                request.BannerImageUrl
            );

            await _repository.AddAsync(season);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Success(season.Id);
        }
        catch ( ArgumentException ex )
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}