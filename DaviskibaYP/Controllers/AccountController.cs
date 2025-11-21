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

            var body =
                $"Здравствуйте, {user.Name ?? user.Email}!\n\n" +
                "Вы зарегистрировались на сайте GastroFest.\n" +
                "Для подтверждения электронной почты перейдите по ссылке:\n\n" +
                $"{confirmUrl}\n\n" +
                "Если вы не регистрировались на нашем сайте, просто игнорируйте это письмо.";

            try
            {
                await _emailSender.SendAsync(user.Email, "Подтверждение регистрации на GastroFest", body, ct);
            }
            catch
            {
                // можно залогировать ошибку, но регистрацию мы не отменяем
            }

            return Json(new
            {
                success = true,
                message = "Регистрация почти завершена. На ваш email отправлено письмо с ссылкой для подтверждения."
            });
        }


        // ===== ПОДТВЕРЖДЕНИЕ EMAIL =====
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(int userId, string code, CancellationToken ct)
        {
            var (ok, error, user) = await _userService.ConfirmEmailAsync(userId, code, ct);

            if (!ok || user == null)
            {
                // для простоты просто текст, можно сделать отдельное представление
                return Content(error ?? "Не удалось подтвердить email.");
            }

            // после успешного подтверждения сразу логиним пользователя
            await SignInUserAsync(user);

            return Content("Email успешно подтверждён. Вы вошли в систему.");
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
