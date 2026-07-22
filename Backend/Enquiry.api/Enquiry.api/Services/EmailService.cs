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
        var section = config.GetSection("Smtp");
        _smtpHost = section["Host"] ?? throw new InvalidOperationException("SMTP Host not configured.");
        _smtpPort = int.Parse(section["Port"] ?? "587");
        _smtpUser = section["User"] ?? "";
        _smtpPass = section["Pass"] ?? "";
        _fromEmail = section["FromEmail"] ?? _smtpUser;
        _fromName = section["FromName"] ?? "EnquiryPro";
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
