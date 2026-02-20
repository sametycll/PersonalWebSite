using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace SyweachWeb.Services;

public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string AppPassword { get; set; } = string.Empty;
    public string ReceiverEmail { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
}

public interface IEmailService
{
    Task SendEmailAsync(string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        message.To.Add(new MailboxAddress("Receiver", _emailSettings.ReceiverEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        
        // Sunucu sertifikası adı uyuşmazlığı (mail.syweach.com vs pleskserver...) gibi durumları önlemek için
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        
        // Port 465 usually requires UseSsl: true (Implicit SSL)
        // Port 587 usually requires UseSsl: false with SecureSocketOptions.StartTls
        var socketOptions = _emailSettings.SmtpPort == 465 
            ? SecureSocketOptions.SslOnConnect 
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, socketOptions);
        await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.AppPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
