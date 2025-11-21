using DAL.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class ContactService
    {
        private readonly IBaseStorage<ContactMessage> _storage;
        private readonly IEmailSender _emailSender;
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<ContactService> _logger;

        public ContactService(
            IBaseStorage<ContactMessage> storage,
            IEmailSender emailSender,
            SmtpSettings smtpSettings,
            ILogger<ContactService> logger)
        {
            _storage = storage;
            _emailSender = emailSender;
            _smtpSettings = smtpSettings;
            _logger = logger;
        }

        /// <summary>
        /// Возвращает true, если письмо пользователю отправлено без ошибок.
        /// </summary>
        public async Task<bool> SubmitMessageAsync(ContactMessage message, CancellationToken ct = default)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message.CreatedAt = DateTime.UtcNow;

            // 1. Сохраняем в БД
            await _storage.AddAsync(message, ct);

            // 2. Письмо админу
            var adminSubject = "Новое сообщение с сайта GastroFest";
            var adminBody =
                $"Имя: {message.Name}\n" +
                $"Email: {message.Email}\n" +
                $"Дата: {message.CreatedAt:dd.MM.yyyy HH:mm}\n\n" +
                $"Сообщение:\n{message.Message}";

            await _emailSender.SendAsync(_smtpSettings.AdminEmail, adminSubject, adminBody, ct);

            // 3. Письмо пользователю
            var userSubject = "Ваше обращение на сайт GastroFest получено";
            var userBody =
                $"Здравствуйте, {message.Name}!\n\n" +
                "Спасибо за ваше обращение на сайт GastroFest.\n" +
                "Ваш запрос получен и будет обработан в ближайшее время.\n\n" +
                "Текст вашего сообщения:\n" +
                $"{message.Message}\n\n" +
                "С уважением,\nКоманда GastroFest";

            try
            {
                await _emailSender.SendAsync(message.Email, userSubject, userBody, ct);
                return true;    // письмо пользователю ушло
            }
            catch (Exception ex)
            {
                // Сюда попадаем, если:
                // - очень кривой домен (smtp сразу ругнулся),
                // - проблемы с подключением и т.п.
                _logger.LogWarning(ex,
                    "Не удалось отправить подтверждение пользователю на email {Email}", message.Email);

                return false;   // считаем, что email не существует/недоступен
            }
        }

        public Task<List<ContactMessage>> GetAllAsync(CancellationToken ct = default) =>
            _storage.GetAllAsync(ct);

        public Task<bool> DeleteAsync(int id, CancellationToken ct = default) =>
            _storage.DeleteAsync(id, ct);
    }
}
