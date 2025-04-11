using Adly.Application.Repositories.Location;
using Adly.Domain.Entities.Ad;
using Adly.Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Adly.Infrastructure.Persistence.Repositories;

internal sealed class LocationRepository(AdlyDbContext db) 
    :BaseRepository<LocationEntity>(db),ILocationRepository
{
    public async Task CreateAsync(LocationEntity locationEntity, CancellationToken cancellationToken = default)
    {
        await base.AddAsync(locationEntity, cancellationToken);
    }

    public async Task<LocationEntity?> GetLocationByIdAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        return await base.TableNoTracking
            .FirstOrDefaultAsync(c => c.Id.Equals(locationId), cancellationToken: cancellationToken);
    }

    public async Task<bool> IsLocationNameExistsAsync(string locationName, CancellationToken cancellationToken = default)
    {
        return await base
            .TableNoTracking
            .AnyAsync(c => c.Name.Equals(locationName), cancellationToken: cancellationToken);
    }

    public async Task<List<LocationEntity>> GetLocationsByNameAsync(string locationName, CancellationToken cancellationToken = default)
    {
        return await base.TableNoTracking
            .Where(c =>
                c.Name.Contains(locationName))
            .ToListAsync(cancellationToken);
    }

    public async Task<LocationEntity?> GetLocationByIdForEditAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        return await base.Table.FirstOrDefaultAsync(c => c.Id.Equals(locationId),cancellationToken);
    }
}