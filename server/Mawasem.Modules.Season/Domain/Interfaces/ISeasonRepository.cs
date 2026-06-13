// Mawasem.Modules.Season/Domain/Interfaces/ISeasonRepository.cs
using Mawasem.Modules.Season.Domain.Entities;

namespace Mawasem.Modules.Season.Domain.Interfaces;

public interface ISeasonRepository
{
    Task<SeasonEntity?> GetByIdAsync( Guid id );
    Task<SeasonEntity?> GetActiveSeasonAsync();
    Task<List<SeasonEntity>> GetAllAsync();
    Task AddAsync( SeasonEntity season );
    Task UpdateAsync( SeasonEntity season );
    Task SaveChangesAsync();
}