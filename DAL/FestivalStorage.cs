using DAL.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Storage;

public class FestivalStorage : IBaseStorage<Festival>
{
    private readonly GastroFestDbContext _db;

    public FestivalStorage(GastroFestDbContext db)
    {
        _db = db;
    }

    public async Task<Festival> AddAsync(Festival entity, CancellationToken ct = default)
    {
        await _db.Festivals.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Festival> UpdateAsync(Festival entity, CancellationToken ct = default)
    {
        _db.Festivals.Update(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var f = await _db.Festivals.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (f == null)
            return false;

        _db.Festivals.Remove(f);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<Festival?> GetAsync(int id, CancellationToken ct = default) =>
        _db.Festivals
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<Festival>> GetAllAsync(CancellationToken ct = default) =>
        _db.Festivals
            .OrderBy(f => f.StartDate)
            .ToListAsync(ct);
}
