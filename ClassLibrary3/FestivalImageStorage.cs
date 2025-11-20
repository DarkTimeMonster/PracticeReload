using DAL.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Storage;

public class FestivalImageStorage : IBaseStorage<FestivalImage>
{
    private readonly GastroFestDbContext _db;

    public FestivalImageStorage(GastroFestDbContext db)
    {
        _db = db;
    }

    public async Task<FestivalImage> AddAsync(FestivalImage entity, CancellationToken ct = default)
    {
        await _db.FestivalImages.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<FestivalImage> UpdateAsync(FestivalImage entity, CancellationToken ct = default)
    {
        _db.FestivalImages.Update(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var img = await _db.FestivalImages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (img == null) return false;

        _db.FestivalImages.Remove(img);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<FestivalImage?> GetAsync(int id, CancellationToken ct = default) =>
        _db.FestivalImages.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<FestivalImage>> GetAllAsync(CancellationToken ct = default) =>
        _db.FestivalImages
           .OrderBy(x => x.FestivalId)
           .ThenBy(x => x.SortOrder)
           .ToListAsync(ct);
}
