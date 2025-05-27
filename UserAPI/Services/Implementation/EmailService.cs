using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using UserAPI.Models;

namespace UserAPI.Services
{
    public class EmailService
    {
        private readonly SmtpSetting _smtpSetting;

        public EmailService(IOptions<SmtpSetting> smtpOptions)
        {
            _smtpSetting = smtpOptions.Value;
        }

        public async Task SendOtpAsync(string email, string otp)
        {
            var subject = "Your OTP Code";

            var htmlContent = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 50px auto;
            background-color: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            text-align: center;
            padding-bottom: 20px;
        }}
        .otp-code {{
            font-size: 24px;
            color: #333333;
            text-align: center;
            margin: 20px 0;
            font-weight: bold;
        }}
        .footer {{
            text-align: center;
            font-size: 12px;
            color: #777777;
            margin-top: 30px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Fitness Tracking App</h2>
        </div>
        <p>Dear User,</p>
        <p>Your One-Time Password (OTP) is:</p>
        <div class=""otp-code"">{otp}</div>
        <p>This OTP is valid for 5 minutes. Please do not share it with anyone.</p>
        <p>If you did not request this code, please ignore this email.</p>
        <div class=""footer"">
            &copy; {DateTime.UtcNow.Year} Fitness Tracking App. All rights reserved.
        </div>
    //</div>
</body>
</html>";

            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(_smtpSetting.From, "Fitness Tracking App");
                mail.To.Add(email);
                mail.Subject = subject;
                mail.Body = htmlContent;
                mail.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(_smtpSetting.Host, _smtpSetting.Port)
                {
                    Credentials = new NetworkCredential(_smtpSetting.Username, _smtpSetting.Password),
                    EnableSsl = _smtpSetting.EnableSsl
                };

                await smtpClient.SendMailAsync(mail);
                Console.WriteLine("OTP email sent successfully via SMTP.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }
    }
}
