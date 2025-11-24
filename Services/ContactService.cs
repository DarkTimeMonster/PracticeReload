using DAL.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Services.EmailTemplates; // <-- ДОБАВИЛИ ЭТО

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

            var userName = message.Name;
            var userEmail = message.Email;
            var userMessage = message.Message;

            // 2. Письмо админу (HTML-шаблон)
            var adminSubject = "Новое сообщение с сайта GastroFest";
            var adminBody = GastrofestEmailTemplates.BuildContactAdminEmail(
                userName,
                userEmail,
                userMessage
            );

            try
            {
                await _emailSender.SendAsync(_smtpSettings.AdminEmail, adminSubject, adminBody, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Не удалось отправить письмо администратору на {AdminEmail}",
                    _smtpSettings.AdminEmail);
            }

            // 3. Письмо пользователю (HTML-шаблон)
            var userSubject = "Ваше обращение на сайт GastroFest получено";
            var userBody = GastrofestEmailTemplates.BuildContactUserEmail(
                userName,
                userMessage
            );

            try
            {
                await _emailSender.SendAsync(userEmail, userSubject, userBody, ct);
                return true;    // письмо пользователю ушло
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Не удалось отправить подтверждение пользователю на email {Email}", userEmail);

                return false;   // считаем, что email не существует/недоступен
            }
        }

        public Task<List<ContactMessage>> GetAllAsync(CancellationToken ct = default) =>
            _storage.GetAllAsync(ct);

        public Task<bool> DeleteAsync(int id, CancellationToken ct = default) =>
            _storage.DeleteAsync(id, ct);
    }
}
