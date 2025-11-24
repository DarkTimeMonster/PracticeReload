using System.Net;
using System.Net.Mail;

namespace Services;

public class GmailEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;

    public GmailEmailSender(SmtpSettings settings)
    {
        _settings = settings;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.From),
            Subject = subject,
            Body = body,
            IsBodyHtml = true // <-- вот это
        };

        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }
}
