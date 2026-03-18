using Microsoft.Extensions.Options;
using OnlineCourses.Application.Interfaces.Services;
using OnlineCourses.Infrastructur.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace OnlineCourses.Infrastructur.Services;

internal sealed class EmailService(IOptions<MailSettings> options) : IEmailService
{
    private readonly MailSettings _settings = options.Value;

    public async Task SendAsync(string toEmail, string subject, string htmlBody,
                                CancellationToken ct = default)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(_settings.Host, _settings.Port,
                                SecureSocketOptions.StartTls, ct);

        await smtp.AuthenticateAsync(_settings.From, _settings.Password, ct);
        await smtp.SendAsync(message, ct);
        await smtp.DisconnectAsync(quit: true, ct);
    }
}