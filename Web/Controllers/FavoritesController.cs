using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Security.Claims;

[Authorize]
public class FavoritesController : Controller
{
    private readonly FestivalService _festivalService;

    public FavoritesController(FestivalService festivalService)
    {
        _festivalService = festivalService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return RedirectToAction("Index", "Home");

        var items = await _festivalService.GetFavoritesAsync(userId, ct);


        return View(items);
    }
}
