using DAL;
using DAL.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class FestivalImageService
{
    private readonly IBaseStorage<FestivalImage> _storage;
    private readonly GastroFestDbContext _db;

    public FestivalImageService(IBaseStorage<FestivalImage> storage, GastroFestDbContext db)
    {
        _storage = storage;
        _db = db;
    }

    public Task<List<FestivalImage>> GetByFestivalAsync(int festivalId, CancellationToken ct = default) =>
        _db.FestivalImages
           .Where(x => x.FestivalId == festivalId)
           .OrderBy(x => x.SortOrder)
           .ToListAsync(ct);

    public Task<FestivalImage?> GetAsync(int id, CancellationToken ct = default) =>
        _storage.GetAsync(id, ct);

    public Task<FestivalImage> CreateAsync(FestivalImage img, CancellationToken ct = default)
    {
        if (img.CreatedAt == default)
            img.CreatedAt = DateTime.UtcNow;

        return _storage.AddAsync(img, ct);
    }

    public Task<FestivalImage> UpdateAsync(FestivalImage img, CancellationToken ct = default) =>
        _storage.UpdateAsync(img, ct);

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default) =>
        _storage.DeleteAsync(id, ct);
}
