using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace DaviskibaYP.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ContactService _service;

        public ContactsController(ContactService service)
        {
            _service = service;
        }

        // Админ-страница для просмотра всех сообщений
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var messages = await _service.GetAllAsync(ct);
            return View(messages); // Views/Contacts/Index.cshtml @model List<ContactMessage>
        }

        // Приём сообщения с формы (AJAX)
        [HttpPost]
        [Route("Contacts/Send")]
        [Consumes("application/json")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Send([FromBody] ContactMessage model, CancellationToken ct)
        {
            // Здесь автоматически сработает ContactMessageValidator → ошибки в ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value!.Errors.FirstOrDefault()?.ErrorMessage ?? "Недопустимое значение"
                );

                return Json(new
                {
                    success = false,
                    errors
                });
            }

            await _service.SubmitMessageAsync(model, ct);

            return Json(new
            {
                success = true,
                message = $"Спасибо, {model.Name}! Ваше сообщение успешно отправлено."
            });
        }
    }
}
