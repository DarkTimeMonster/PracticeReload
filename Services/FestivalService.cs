using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DAL;
using DAL.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Services
{
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

        // ===== БЛИЖАЙШИЙ ФЕСТИВАЛЬ =====

        public Task<Festival?> GetNearestAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            return _db.Festivals
                .Where(f => f.StartDate >= now)   // только будущие
                .OrderBy(f => f.StartDate)        // самый близкий по дате начала
                .FirstOrDefaultAsync(ct);
        }

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

        public async Task<(List<Festival> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            string? city,
            string? dateFilter,
        CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 6;

            var query = _db.Festivals.AsQueryable();

            // === поиск по названию ===
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(f => EF.Functions.ILike(f.Title, $"%{term}%"));
            }

            // === фильтр по городу ===
            if (!string.IsNullOrWhiteSpace(city))
            {
                if (city == "other")
                {
                    query = query.Where(f =>
                        f.City != "Москва" &&
                        f.City != "Санкт-Петербург");
                }
                else
                {
                    query = query.Where(f => f.City == city);
                }
            }

            // === фильтр по датам (UTC!) ===
            if (!string.IsNullOrWhiteSpace(dateFilter))
            {
                // Берём сегодняшнюю дату в UTC
                var todayUtc = DateTime.UtcNow;

                // Первый день текущего месяца в UTC
                var firstCurrentMonth = new DateTime(
                    todayUtc.Year,
                    todayUtc.Month,
                    1,
                    0, 0, 0,
                    DateTimeKind.Utc);

                // Первый день следующего месяца в UTC
                var firstNextMonth = firstCurrentMonth.AddMonths(1);
                // Первый день месяца после следующего
                var firstAfterNext = firstNextMonth.AddMonths(1);

                if (dateFilter == "currentMonth")
                {
                    query = query.Where(f =>
                        f.StartDate >= firstCurrentMonth &&
                        f.StartDate < firstNextMonth);
                }
                else if (dateFilter == "nextMonth")
                {
                    query = query.Where(f =>
                        f.StartDate >= firstNextMonth &&
                        f.StartDate < firstAfterNext);
                }
            }

            // сортируем по дате начала
            query = query.OrderBy(f => f.StartDate);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}
