using Adly.Application.Repositories.Ad;
using Adly.Application.Repositories.Category;
using Adly.Application.Repositories.Common;
using Adly.Application.Repositories.Location;

namespace Adly.Infrastructure.Persistence.Repositories.Common;

public class UnitOfWork:IUnitOfWork
{
    private readonly AdlyDbContext _db;
    
    
    public UnitOfWork(AdlyDbContext db)
    {
        _db = db;
        LocationRepository = new LocationRepository(db);
        CategoryRepository = new CategoryRepository(db);
        AdRepository = new AdRepository(db);
    }
    
    public void Dispose()
    {
        _db.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _db.DisposeAsync();
    }

    public ILocationRepository LocationRepository { get; }
    public ICategoryRepository CategoryRepository { get; }
    public IAdRepository AdRepository { get; }

   

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }
}