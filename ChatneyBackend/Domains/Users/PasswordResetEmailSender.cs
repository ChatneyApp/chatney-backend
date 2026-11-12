using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;

namespace ChatneyBackend.Domains.Users;

public class PasswordResetOptions
{
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
    public string FromAddress { get; set; } = "noreply@chatney.local";
    public string FromName { get; set; } = "Chatney";
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 1025;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool EnableSsl { get; set; }
}

public interface IPasswordResetEmailSender
{
    Task SendAsync(User user, Guid code, CancellationToken cancellationToken = default);
}

public sealed class SmtpPasswordResetEmailSender(
    IOptions<PasswordResetOptions> options) : IPasswordResetEmailSender
{
    private readonly PasswordResetOptions _options = options.Value;

    public async Task SendAsync(
        User user,
        Guid code,
        CancellationToken cancellationToken = default)
    {
        var resetUrl = $"{_options.FrontendBaseUrl.TrimEnd('/')}/reset-password?code={code:D}";
        var encodedUrl = HtmlEncoder.Default.Encode(resetUrl);
        var encodedNickname = HtmlEncoder.Default.Encode(user.Nickname);

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = "Reset your Chatney password",
            Body = $"""
                <p>Hello {encodedNickname},</p>
                <p>Use the link below to reset your Chatney password:</p>
                <p><a href="{encodedUrl}">{encodedUrl}</a></p>
                <p>This link works for 1 hour and can only be used once.</p>
                <p>If you did not request this reset, you can ignore this email.</p>
                """,
            IsBodyHtml = true,
        };
        message.To.Add(user.Email);

        using var smtp = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
            UseDefaultCredentials = string.IsNullOrWhiteSpace(_options.SmtpUsername),
        };

        if (!smtp.UseDefaultCredentials)
        {
            smtp.Credentials = new NetworkCredential(
                _options.SmtpUsername,
                _options.SmtpPassword);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await smtp.SendMailAsync(message, cancellationToken);
    }
}
