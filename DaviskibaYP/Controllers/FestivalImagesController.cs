using Domain.Entities;
using Domain.ViewModels.Festivals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

[Authorize(Roles = "Admin")]
public class FestivalImagesController : Controller
{
    private readonly FestivalImageService _images;
    private readonly FestivalService _festivals;

    public FestivalImagesController(FestivalImageService images, FestivalService festivals)
    {
        _images = images;
        _festivals = festivals;
    }

    // список картинок для одного фестиваля
    [HttpGet]
    public async Task<IActionResult> Index(int festivalId, CancellationToken ct)
    {
        var fest = await _festivals.GetAsync(festivalId, ct);
        if (fest == null) return NotFound();

        var imgs = await _images.GetByFestivalAsync(festivalId, ct);
        ViewBag.Festival = fest;
        return View(imgs); // Views/FestivalImages/Index.cshtml, @model List<FestivalImage>
    }

    [HttpGet]
    public IActionResult Create(int festivalId)
    {
        return View(new FestivalImageEditViewModel
        {
            FestivalId = festivalId,
            SortOrder = 0
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FestivalImageEditViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var entity = new FestivalImage
        {
            FestivalId = vm.FestivalId,
            Url = vm.Url,
            Alt = vm.Alt,
            SortOrder = vm.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        await _images.CreateAsync(entity, ct);
        return RedirectToAction("Edit", "Festivals", new { id = vm.FestivalId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var entity = await _images.GetAsync(id, ct);
        if (entity == null) return NotFound();

        var vm = new FestivalImageEditViewModel
        {
            Id = entity.Id,
            FestivalId = entity.FestivalId,
            Url = entity.Url,
            Alt = entity.Alt,
            SortOrder = entity.SortOrder
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(FestivalImageEditViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var entity = await _images.GetAsync(vm.Id, ct);
        if (entity == null) return NotFound();

        entity.Url = vm.Url;
        entity.Alt = vm.Alt;
        entity.SortOrder = vm.SortOrder;

        await _images.UpdateAsync(entity, ct);
        return RedirectToAction("Edit", "Festivals", new { id = entity.FestivalId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int festivalId, CancellationToken ct)
    {
        await _images.DeleteAsync(id, ct);
        return RedirectToAction("Edit", "Festivals", new { id = festivalId });
    }
}
