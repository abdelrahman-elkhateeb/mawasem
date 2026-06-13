using Mawasem.Infrastructure.Persistence;
using Mawasem.Modules.Season.Domain.Entities;
using Mawasem.Modules.Season.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Repositories;

public class SeasonRepository : ISeasonRepository
{
    private readonly MawasemDbContext _context;

    public SeasonRepository( MawasemDbContext context )
    {
        _context = context;
    }

    public async Task<SeasonEntity?> GetByIdAsync( Guid id )
        => await _context.Seasons.FirstOrDefaultAsync(s => s.Id == id);

    public async Task<SeasonEntity?> GetActiveSeasonAsync()
        => await _context.Seasons.FirstOrDefaultAsync(s => s.IsActive);

    public async Task<List<SeasonEntity>> GetAllAsync()
        => await _context.Seasons.ToListAsync();

    public async Task AddAsync( SeasonEntity season )
        => await _context.Seasons.AddAsync(season);

    public async Task UpdateAsync( SeasonEntity season )
        => _context.Seasons.Update(season);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}