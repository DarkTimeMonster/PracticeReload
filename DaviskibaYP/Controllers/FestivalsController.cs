using Domain.Entities;
using Domain.ViewModels.Festivals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Security.Claims;

public class FestivalsController : Controller
{
    private readonly FestivalService _festivalService;

    public FestivalsController(FestivalService festivalService)
    {
        _festivalService = festivalService;
    }
    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> Index(
    string? search,
    string? city,
    string? dateFilter,
    int page = 1,
    int pageSize = 6,
    CancellationToken ct = default)
    {
        var (items, totalCount) = await _festivalService.GetPagedAsync(
            page, pageSize, search, city, dateFilter, ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var vm = new FestivalListViewModel
        {
            Festivals = items,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalCount = totalCount,
            Search = search,
            City = city,
            DateFilter = dateFilter
        };

        return View(vm);
    }

    // ДЕТАЛИ
    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var fest = await _festivalService.GetByIdWithImagesAsync(id, ct);
        if (fest == null) return NotFound();

        bool isFavorite = false;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId))
                isFavorite = await _festivalService.IsFavoriteAsync(id, userId, ct);
        }

        var vm = new FestivalDetailsViewModel
        {
            Id = fest.Id,
            Title = fest.Title,
            Description = fest.Description,
            City = fest.City,
            Country = fest.Country,
            StartDate = fest.StartDate,
            EndDate = fest.EndDate,
            CoverUrl = fest.CoverUrl,
            IsFavorite = isFavorite,
            Images = fest.Images
                .OrderBy(i => i.SortOrder)
                .Select(i => new FestivalDetailsViewModel.ImageItem
                {
                    Url = i.Url,
                    Alt = i.Alt
                })
                .ToList()
        };

        return View(vm);
    }

    // Toggle избранного (оставляем как есть)
    [Authorize]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ToggleFavorite([FromBody] int id, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return Json(new { success = false, error = "Пользователь не найден" });

        var isFavorite = await _festivalService.ToggleFavoriteAsync(id, userId, ct);
        return Json(new { success = true, isFavorite });
    }

    // ===== СОЗДАНИЕ =====

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FestivalEditViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        // DateTimeKind, чтобы Нpgsql не орал про timestamptz
        var startUtc = DateTime.SpecifyKind(vm.StartDate, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(vm.EndDate, DateTimeKind.Utc);

        var fest = new Festival
        {
            Title = vm.Title,
            Description = vm.Description,
            City = vm.City,
            Country = vm.Country,
            StartDate = startUtc,
            EndDate = endUtc,
            CoverUrl = vm.CoverUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _festivalService.CreateAsync(fest, ct);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Festivals/Edit/5
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var fest = await _festivalService.GetAsync(id, ct);
        if (fest == null) return NotFound();

        var vm = new FestivalEditViewModel
        {
            Id = fest.Id,
            Title = fest.Title,
            Description = fest.Description,
            City = fest.City,
            Country = fest.Country,
            StartDate = fest.StartDate,
            EndDate = fest.EndDate,
            CoverUrl = fest.CoverUrl
        };

        return View(vm); // Views/Festivals/Edit.cshtml
    }

    // POST: /Festivals/Edit
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(FestivalEditViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var fest = await _festivalService.GetAsync(vm.Id, ct);
        if (fest == null) return NotFound();

        fest.Title = vm.Title;
        fest.Description = vm.Description;
        fest.City = vm.City;
        fest.Country = vm.Country;
        fest.StartDate = DateTime.SpecifyKind(vm.StartDate, DateTimeKind.Utc);
        fest.EndDate = DateTime.SpecifyKind(vm.EndDate, DateTimeKind.Utc);
        fest.CoverUrl = vm.CoverUrl;

        await _festivalService.UpdateAsync(fest, ct);

        return RedirectToAction(nameof(Index));
    }

    // POST: /Festivals/Delete/5
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _festivalService.DeleteAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> GetAllJson(CancellationToken ct)
    {
        var list = await _festivalService.GetAllAsync(ct);

        var result = list.Select(f => new
        {
            id = f.Id,
            title = f.Title,
            description = f.Description,
            city = f.City,
            country = f.Country,
            startDate = f.StartDate, // в JSON уйдёт ISO-строка
            endDate = f.EndDate,
            coverUrl = f.CoverUrl
        });

        return Json(result);
    }

}
