using E_Commerce.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace E_Commerce.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> _logger)
        {
            _configuration = configuration;
            this._logger = _logger;
        }

        public async Task SendPasswordResetOtpAsync(string email, string otp)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? smtpUsername;
                var fromName = _configuration["EmailSettings:FromName"] ?? "E-Commerce App";

                var subject = "Password Reset OTP";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .otp-box {{ background-color: #ffffff; border: 2px solid #4CAF50; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 8px; margin: 20px 0; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
        .warning {{ color: #d32f2f; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Password Reset OTP</h1>
        </div>
        <div class=""content"">
            <p>Hello,</p>
            <p>We received a request to reset your password. Use the following One-Time Password (OTP) to reset your password:</p>
            <div class=""otp-box"">
                {otp}
            </div>
            <p class=""warning"">This OTP will expire in 10 minutes.</p>
            <p class=""warning"">If you did not request this password reset, please ignore this email.</p>
            <p>For security reasons, this OTP can only be used once.</p>
            <p><strong>Never share this OTP with anyone.</strong></p>
        </div>
        <div class=""footer"">
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

                // If SMTP is not configured, log the email instead
                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogWarning("Email service not configured. Password reset OTP for {Email}: {Otp}", email, otp);
                    return;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(email);

                await client.SendMailAsync(message);
                _logger.LogInformation("Password reset OTP sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset OTP to {Email}", email);
                // Don't throw - we don't want to expose email service failures to users
            }
        }
    }
}

