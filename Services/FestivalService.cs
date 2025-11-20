using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DAL;
using DAL.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class FestivalService
{
    private readonly IBaseStorage<Festival> _storage;
    private readonly GastroFestDbContext _db;

    public FestivalService(IBaseStorage<Festival> storage, GastroFestDbContext db)
    {
        _storage = storage;
        _db = db;
    }

    // ===== БАЗОВЫЙ CRUD =====

    public Task<List<Festival>> GetAllAsync(CancellationToken ct = default) =>
        _storage.GetAllAsync(ct);

    public Task<Festival?> GetAsync(int id, CancellationToken ct = default) =>
        _storage.GetAsync(id, ct);

    public Task<Festival> CreateAsync(Festival festival, CancellationToken ct = default) =>
        _storage.AddAsync(festival, ct);

    public Task<Festival> UpdateAsync(Festival festival, CancellationToken ct = default) =>
        _storage.UpdateAsync(festival, ct);

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default) =>
        _storage.DeleteAsync(id, ct);

    // ===== ДЕТАЛИ С КАРТИНКАМИ =====

    /// <summary>
    /// Получить фестиваль по Id вместе с картинками (Images).
    /// </summary>
    public Task<Festival?> GetByIdWithImagesAsync(int id, CancellationToken ct = default) =>
        _db.Festivals
           .Include(f => f.Images)
           .FirstOrDefaultAsync(f => f.Id == id, ct);

    // ===== ИЗБРАННОЕ (UserFavorite) =====

    /// <summary>
    /// Проверить, находится ли фестиваль в избранном у пользователя.
    /// </summary>
    public Task<bool> IsFavoriteAsync(int festivalId, int userId, CancellationToken ct = default) =>
        _db.UserFavorites
           .AnyAsync(x => x.FestivalId == festivalId && x.UserId == userId, ct);

    /// <summary>
    /// Переключить избранное: если не было — добавить, если было — удалить.
    /// Возвращает новое состояние: true = теперь в избранном, false = удалён.
    /// </summary>
    public async Task<bool> ToggleFavoriteAsync(int festivalId, int userId, CancellationToken ct = default)
    {
        var fav = await _db.UserFavorites
            .FirstOrDefaultAsync(x => x.FestivalId == festivalId && x.UserId == userId, ct);

        if (fav == null)
        {
            _db.UserFavorites.Add(new UserFavorite
            {
                FestivalId = festivalId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);
            return true; // теперь в избранном
        }

        _db.UserFavorites.Remove(fav);
        await _db.SaveChangesAsync(ct);
        return false; // теперь не в избранном
    }
}
