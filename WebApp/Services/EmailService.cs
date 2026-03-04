using Microsoft.Extensions.Options;
using System.Net.Mail;
using WebApp.Settings;

namespace WebApp.Services;

public class EmailService : IEmailService
{
    private readonly IOptions<SmtpSettings> _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MailMessage
        {
            To = { new MailAddress(toEmail) },
            From = new MailAddress(_smtpSettings.Value.From),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        using (var smtpClient = new SmtpClient(
            _smtpSettings.Value.Host,
            _smtpSettings.Value.Port))
        {
            smtpClient.Credentials = new System.Net.NetworkCredential(
                _smtpSettings.Value.Username,
                _smtpSettings.Value.Password);
            await smtpClient.SendMailAsync(message);
        }
    }
}
