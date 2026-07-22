using System.Net;
using System.Net.Mail;

namespace Enquiry.api.EmailService;

public class EmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration config)
    {
        _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST")
            ?? config["Smtp:Host"]
            ?? throw new InvalidOperationException("SMTP Host not configured. Set SMTP_HOST env var or Smtp:Host in appsettings.");
        _smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")
            ?? config["Smtp:Port"]
            ?? "587");
        _smtpUser = Environment.GetEnvironmentVariable("SMTP_USER")
            ?? config["Smtp:User"]
            ?? "";
        _smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS")
            ?? config["Smtp:Pass"]
            ?? "";
        _fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL")
            ?? config["Smtp:FromEmail"]
            ?? _smtpUser;
        _fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME")
            ?? config["Smtp:FromName"]
            ?? "EnquiryPro";
    }

    public async Task SendAsync(string toEmail, string subject, string body)
    {
        using var client = new SmtpClient(_smtpHost, _smtpPort);
        client.EnableSsl = true;
        if (!string.IsNullOrEmpty(_smtpUser))
            client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);

        var msg = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        msg.To.Add(toEmail);

        await client.SendMailAsync(msg);
    }
}
