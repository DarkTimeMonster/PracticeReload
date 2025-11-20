using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace DaviskibaYP.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserService _userService;

        public ProfileController(UserService userService)
        {
            _userService = userService;
        }

        // ===== СТРАНИЦА ПРОФИЛЯ =====
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Index", "Home");

            var user = await _userService.GetByIdAsync(userId, ct);
            if (user == null)
                return RedirectToAction("Index", "Home");

            var vm = new ProfilePageViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            ViewBag.ProfileMessage = TempData["ProfileMessage"];
            ViewBag.ProfileError = TempData["ProfileError"];

            return View(vm);
        }

        // ===== ОБНОВЛЕНИЕ ИМЕНИ/EMAIL =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfilePageViewModel model, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                ViewBag.ProfileError = "Проверьте правильность заполнения формы.";
                return View("Index", model);
            }

            var (ok, error) = await _userService.UpdateProfileAsync(
                userId,
                model.Name,
                model.Email,
                ct);

            if (!ok)
            {
                TempData["ProfileError"] = error ?? "Не удалось обновить профиль.";
            }
            else
            {
                TempData["ProfileMessage"] = "Профиль успешно обновлён.";
            }

            return RedirectToAction("Index");
        }

        // ===== СМЕНА ПАРОЛЯ =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                TempData["ProfileError"] = "Проверьте правильность заполнения формы.";
                return RedirectToAction("Index");
            }

            var (ok, error) = await _userService.ChangePasswordAsync(
                userId,
                model.CurrentPassword,
                model.NewPassword,
                ct);

            if (!ok)
            {
                TempData["ProfileError"] = error ?? "Не удалось сменить пароль.";
            }
            else
            {
                TempData["ProfileMessage"] = "Пароль успешно изменён.";
            }

            return RedirectToAction("Index");
        }
    }
}
