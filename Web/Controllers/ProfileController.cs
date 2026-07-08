using Domain.Entities;
using Domain.ViewModels.Profile;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Account;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

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

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfilePageViewModel model, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Проверьте правильность заполнения формы.";
                return RedirectToAction("Index");
            }

            var (ok, error) = await _userService.UpdateProfileAsync(
                userId,
                model.Name,
                model.Email,
                ct);

            if (!ok)
            {
                TempData["Error"] = error ?? "Не удалось обновить профиль.";
                return RedirectToAction("Index");
            }

            // подтягиваем обновлённого пользователя и ОБНОВЛЯЕМ куку
            var updatedUser = await _userService.GetByIdAsync(userId, ct);
            if (updatedUser != null)
            {
                await SignInUserAsync(updatedUser);
            }

            TempData["Success"] = "Профиль успешно обновлён.";
            return RedirectToAction("Index");
        }
        private async Task SignInUserAsync(User user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, string.IsNullOrEmpty(user.Name) ? user.Email : user.Name),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, string.IsNullOrEmpty(user.Role) ? "Customer" : user.Role)
    };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Проверьте правильность заполнения формы.";
                return RedirectToAction("Index");
            }

            var (ok, error) = await _userService.ChangePasswordAsync(
                userId,
                model.CurrentPassword,
                model.NewPassword,
                ct);

            if (!ok)
                TempData["Error"] = error ?? "Не удалось сменить пароль.";
            else
                TempData["Success"] = "Пароль успешно изменён.";

            return RedirectToAction("Index");
        }
    }
}
