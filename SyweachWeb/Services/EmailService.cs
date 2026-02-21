using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string subject, string body)
    {
        string currentStep = "Başlatılıyor";
        
        _logger.LogInformation("========== E-POSTA GÖNDERİM BAŞLANGICI ==========");
        _logger.LogInformation("SMTP Sunucu: {Server}", _emailSettings.SmtpServer);
        _logger.LogInformation("SMTP Port: {Port}", _emailSettings.SmtpPort);
        _logger.LogInformation("Gönderen: {Sender}", _emailSettings.SenderEmail);
        _logger.LogInformation("Alıcı: {Receiver}", _emailSettings.ReceiverEmail);
        _logger.LogInformation("SSL Etkin: {EnableSsl}", _emailSettings.EnableSsl);

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
        
        // Bağlantı zaman aşımı - 30 saniye (sunucu yanıt vermezse takılmasın)
        client.Timeout = 30000;
        
        // Sunucu sertifikası adı uyuşmazlığı (mail.syweach.com vs pleskserver...) gibi durumları önlemek için
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        
        // Port 465 → Implicit SSL (SslOnConnect)
        // Port 587 → STARTTLS
        var socketOptions = _emailSettings.SmtpPort == 465 
            ? SecureSocketOptions.SslOnConnect 
            : SecureSocketOptions.StartTls;

        try
        {
            // ADIM 1: DNS Çözümleme Testi
            currentStep = "DNS_COZUMLEME";
            _logger.LogInformation("[ADIM 1/5] DNS çözümleme yapılıyor: {Server}", _emailSettings.SmtpServer);
            try
            {
                var dnsResult = await System.Net.Dns.GetHostAddressesAsync(_emailSettings.SmtpServer);
                _logger.LogInformation("[ADIM 1/5] DNS başarılı. IP adresleri: {IPs}", 
                    string.Join(", ", dnsResult.Select(ip => ip.ToString())));
            }
            catch (Exception dnsEx)
            {
                _logger.LogError(dnsEx, "[ADIM 1/5] DNS ÇÖZÜMLEME BAŞARISIZ: {Server}", _emailSettings.SmtpServer);
                throw new Exception($"DNS çözümleme başarısız: '{_emailSettings.SmtpServer}' sunucusu bulunamadı. " +
                    $"Sunucu adını kontrol edin. DNS Hata: {dnsEx.Message}", dnsEx);
            }

            // ADIM 2: TCP/SSL Bağlantısı
            currentStep = "SMTP_BAGLANTI";
            _logger.LogInformation("[ADIM 2/5] SMTP bağlantısı kuruluyor: {Server}:{Port} ({Options})", 
                _emailSettings.SmtpServer, _emailSettings.SmtpPort, socketOptions);
            
            var connectStartTime = DateTime.UtcNow;
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, socketOptions);
            var connectDuration = (DateTime.UtcNow - connectStartTime).TotalMilliseconds;
            
            _logger.LogInformation("[ADIM 2/5] SMTP bağlantısı BAŞARILI ({Duration}ms). IsConnected: {Connected}, IsSecure: {Secure}", 
                connectDuration, client.IsConnected, client.IsSecure);

            // ADIM 3: Kimlik Doğrulama
            currentStep = "KIMLIK_DOGRULAMA";
            _logger.LogInformation("[ADIM 3/5] Kimlik doğrulama yapılıyor: {Email}", _emailSettings.SenderEmail);
            await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.AppPassword);
            _logger.LogInformation("[ADIM 3/5] Kimlik doğrulama BAŞARILI. IsAuthenticated: {Auth}", client.IsAuthenticated);

            // ADIM 4: E-posta Gönderimi
            currentStep = "EPOSTA_GONDERIM";
            _logger.LogInformation("[ADIM 4/5] E-posta gönderiliyor...");
            await client.SendAsync(message);
            _logger.LogInformation("[ADIM 4/5] E-posta BAŞARIYLA gönderildi!");

            // ADIM 5: Bağlantı Kapatma
            currentStep = "BAGLANTI_KAPATMA";
            _logger.LogInformation("[ADIM 5/5] SMTP bağlantısı kapatılıyor...");
            await client.DisconnectAsync(true);
            _logger.LogInformation("[ADIM 5/5] Bağlantı kapatıldı. İşlem tamamlandı.");
            _logger.LogInformation("========== E-POSTA GÖNDERİM TAMAMLANDI ==========");
        }
        catch (Exception ex) when (currentStep != "DNS_COZUMLEME" || ex.InnerException == null)
        {
            _logger.LogError(ex, "SMTP işlemi sırasında HATA! Adım: {Step}, Sunucu: {Server}:{Port}, SSL: {Options}. Hata Tipi: {ExType}, Mesaj: {Error}", 
                currentStep, _emailSettings.SmtpServer, _emailSettings.SmtpPort, socketOptions, ex.GetType().FullName, ex.Message);
            
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner Exception Tipi: {InnerType}, Mesaj: {InnerMsg}", 
                    ex.InnerException.GetType().FullName, ex.InnerException.Message);
            }
            
            // Hangi adımda hata olduğunu belirten detaylı exception fırlat
            throw new Exception(
                $"[{currentStep}] SMTP hatası: {ex.Message} | " +
                $"Sunucu: {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort} | " +
                $"SSL: {socketOptions} | " +
                $"Hata Tipi: {ex.GetType().Name}" +
                (ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : ""),
                ex);
        }
    }
}
