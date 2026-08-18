using System.Net;
using System.Net.Mail;

namespace BookStore.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            try
            {
                var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
                var smtpPort = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
                var smtpUser = _configuration["Smtp:User"];
                var smtpPass = _configuration["Smtp:Pass"];

                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    _logger.LogInformation("SMTP not configured. Email mock logged to: {Email}, Subject: {Subject}\nMessage: {Msg}",
                        toEmail, subject, htmlMessage);
                    return true;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUser, "MindBook Store"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode)
        {
            string subject = "MindBook - Mã xác thực OTP của bạn";
            string body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 8px;'>
                    <h2 style='color: #C92127; text-align: center;'>MINDBOOK STORE</h2>
                    <p>Xin chào quý khách,</p>
                    <p>Mã xác thực OTP của bạn là:</p>
                    <div style='text-align: center; margin: 25px 0;'>
                        <span style='display: inline-block; font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #C92127; background: #fdf2f2; padding: 12px 24px; border-radius: 6px; border: 1px dashed #C92127;'>
                            {otpCode}
                        </span>
                    </div>
                    <p style='color: #666; font-size: 13px;'>Mã này có hiệu lực trong vòng 5 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p style='font-size: 12px; color: #999; text-align: center;'>Trân trọng,<br>Đội ngũ MindBook Store</p>
                </div>";

            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}
