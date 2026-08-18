namespace BookStore.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlMessage);
        Task<bool> SendOtpEmailAsync(string toEmail, string otpCode);
    }
}
