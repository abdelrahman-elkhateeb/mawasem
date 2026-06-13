using Mawasem.Modules.Season.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Persistence;

public class MawasemDbContext : DbContext
{
    public MawasemDbContext( DbContextOptions<MawasemDbContext> options )
        : base(options) { }

    public DbSet<SeasonEntity> Seasons => Set<SeasonEntity>();

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MawasemDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}