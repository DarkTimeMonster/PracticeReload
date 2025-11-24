using Domain.Entities;
using Domain.ViewModels.LoginAndRegistration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Services.EmailTemplates;


namespace DaviskibaYP.Controllers
{
    [IgnoreAntiforgeryToken]
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly IEmailSender _emailSender;

        public AccountController(UserService userService, IEmailSender emailSender)
        {
            _userService = userService;
            _emailSender = emailSender;
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
        // ===== РЕГИСТРАЦИЯ С ПОДТВЕРЖДЕНИЕМ EMAIL =====
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

            // Формируем ссылку для подтверждения email
            var confirmUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, code = user.EmailConfirmationCode },
                Request.Scheme);

            // --- ИСПОЛЬЗУЕМ HTML-ШАБЛОН ДЛЯ ПИСЬМА ---
            var displayName = string.IsNullOrWhiteSpace(user.Name) ? user.Email : user.Name;
            var bodyHtml = GastrofestEmailTemplates.BuildRegistrationEmail(displayName, confirmUrl);

            try
            {
                await _emailSender.SendAsync(
                    user.Email,
                    "Подтверждение регистрации на GastroFest",
                    bodyHtml,
                    ct);
            }
            catch
            {
                // можно залогировать, но регистрацию не отменяем
            }

            return Json(new
            {
                success = true,
                message = "Регистрация почти завершена. На ваш email отправлено письмо с ссылкой для подтверждения."
            });
        }



        // ===== ПОДТВЕРЖДЕНИЕ EMAIL =====
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(int userId, string code, CancellationToken ct)
        {
            var (ok, error, user) = await _userService.ConfirmEmailAsync(userId, code, ct);

            if (!ok || user == null)
            {
                TempData["Error"] = error ?? "Не удалось подтвердить email.";
                return RedirectToAction("Index", "Home");
            }

            await SignInUserAsync(user);

            TempData["Success"] = "Email успешно подтверждён. Вы вошли в систему.";
            return RedirectToAction("Index", "Home");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Success"] = "Вы вышли из аккаунта.";
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
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse", "Account", new { returnUrl })
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse(string returnUrl = "/", CancellationToken ct = default)
        {
            var principal = HttpContext.User;

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = principal.Identity?.Name ?? email;

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Не удалось получить email из Google-аккаунта.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userService.GetOrCreateFromGoogleAsync(email, name, ct);

            await SignInUserAsync(user);

            TempData["Success"] = $"Вы вошли как {user.Name ?? user.Email} через Google.";

            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = "/";

            return LocalRedirect(returnUrl);
        }



    }
}
