using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.ViewModels.LoginAndRegistration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace DaviskibaYP.Controllers
{
    [IgnoreAntiforgeryToken]
    public class AccountController : Controller
    {
        private readonly UserService _userService;

        public AccountController(UserService userService)
        {
            _userService = userService;
        }

        // ===== ВХОД =====
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.FirstOrDefault()?.ErrorMessage ?? "Недопустимое значение"
                );

                return Json(new
                {
                    success = false,
                    errors
                });
            }

            var (ok, error, user) = await _userService.LoginAsync(model.Email, model.Password, ct);

            if (!ok || user == null)
            {
                ModelState.AddModelError(string.Empty, error ?? "Ошибка входа");

                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.FirstOrDefault()?.ErrorMessage ?? "Недопустимое значение"
                );

                return Json(new
                {
                    success = false,
                    errors
                });
            }

            await SignInUserAsync(user);

            // можно вернуть имя, если захочешь потом без reload обновлять шапку
            return Json(new
            {
                success = true,
                userName = string.IsNullOrEmpty(user.Name) ? user.Email : user.Name
            });
        }

        // ===== РЕГИСТРАЦИЯ + АВТО-ЛОГИН =====
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.FirstOrDefault()?.ErrorMessage ?? "Недопустимое значение"
                );

                return Json(new
                {
                    success = false,
                    errors
                });
            }

            var (ok, error, user) = await _userService.RegisterAsync(
                model.Name,
                model.Email,
                model.Password,
                ct
            );

            if (!ok || user == null)
            {
                ModelState.AddModelError(nameof(model.Email), error ?? "Ошибка регистрации");

                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.FirstOrDefault()?.ErrorMessage ?? "Недопустимое значение"
                );

                return Json(new
                {
                    success = false,
                    errors
                });
            }

            await SignInUserAsync(user);

            return Json(new
            {
                success = true,
                userName = string.IsNullOrEmpty(user.Name) ? user.Email : user.Name
            });
        }

        // ===== ВЫХОД =====
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // просто уходим на главную
            return RedirectToAction("Index", "Home");
        }

        // ===== ХЕЛПЕР ДЛЯ ЛОГИНА =====

        private async Task SignInUserAsync(User user)
        {
            var userId = user.Id;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
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

    }
}
