using System.Net;
using System.Net.Mail;

namespace AppointmentApi.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var useMock = _configuration.GetValue<bool>("EmailSettings:UseMockService");

            if (useMock)
            {
                // Test Modu: E-postayı terminale yazdır.
                _logger.LogInformation("\n================ MOCK EMAIL GÖNDERİLDİ ================\n" +
                                       $"Kime: {toEmail}\n" +
                                       $"Konu: {subject}\n" +
                                       $"İçerik: {body}\n" +
                                       "====================================================\n");
                return;
            }

            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var password = _configuration["EmailSettings:Password"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail))
                {
                    _logger.LogWarning("Email ayarları eksik. E-posta gönderilemedi.");
                    return;
                }

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta gönderilirken bir hata oluştu.");
            }
        }
    }
}
