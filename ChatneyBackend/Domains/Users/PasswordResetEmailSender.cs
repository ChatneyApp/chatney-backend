using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using ChatneyBackend.Infra;
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
    IOptions<PasswordResetOptions> options,
    AppRepos repos) : IPasswordResetEmailSender
{
    private readonly PasswordResetOptions _options = options.Value;

    public async Task SendAsync(
        User user,
        Guid code,
        CancellationToken cancellationToken = default)
    {
        var configNames = new[]
        {
            Configs.DomainSettings.SmtpEnabled,
            Configs.DomainSettings.SystemDomainName,
            Configs.DomainSettings.SmtpHost,
            Configs.DomainSettings.SmtpPort,
            Configs.DomainSettings.SmtpUsername,
            Configs.DomainSettings.SmtpPassword,
            Configs.DomainSettings.SmtpEnableSsl,
            Configs.DomainSettings.SmtpFromAddress,
        };
        var configs = (await repos.Configs.GetList(
                "SELECT * FROM configs WHERE name = ANY(@Names)",
                new { Names = configNames }))
            .ToDictionary(config => config.Name, config => config.Value);

        var smtpEnabled = GetConfig(configs, Configs.DomainSettings.SmtpEnabled);

        if (smtpEnabled != null && !bool.Parse(smtpEnabled))
        {
            throw new InvalidOperationException("SMTP is disabled.");
        }

        var frontendBaseUrl = GetConfig(configs, Configs.DomainSettings.SystemDomainName)
            ?? _options.FrontendBaseUrl;
        var smtpHost = GetConfig(configs, Configs.DomainSettings.SmtpHost) ?? _options.SmtpHost;
        var smtpPort = int.TryParse(
            GetConfig(configs, Configs.DomainSettings.SmtpPort),
            out var configuredPort)
            ? configuredPort
            : _options.SmtpPort;
        var smtpUsername = GetConfig(configs, Configs.DomainSettings.SmtpUsername)
            ?? _options.SmtpUsername;
        var smtpPassword = GetConfig(configs, Configs.DomainSettings.SmtpPassword)
            ?? _options.SmtpPassword;
        var enableSsl = bool.TryParse(
            GetConfig(configs, Configs.DomainSettings.SmtpEnableSsl),
            out var configuredSsl)
            ? configuredSsl
            : _options.EnableSsl;
        var fromAddress = GetConfig(configs, Configs.DomainSettings.SmtpFromAddress)
            ?? _options.FromAddress;

        var resetUrl = $"{frontendBaseUrl.TrimEnd('/')}/reset-password?code={code:D}";
        var encodedUrl = HtmlEncoder.Default.Encode(resetUrl);
        var encodedNickname = HtmlEncoder.Default.Encode(user.Nickname);

        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress, _options.FromName),
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

        using var smtp = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = enableSsl,
            UseDefaultCredentials = string.IsNullOrWhiteSpace(smtpUsername),
        };

        if (!smtp.UseDefaultCredentials)
        {
            smtp.Credentials = new NetworkCredential(
                smtpUsername,
                smtpPassword);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await smtp.SendMailAsync(message, cancellationToken);
    }

    private static string? GetConfig(Dictionary<string, string> configs, string name)
    {
        return configs.TryGetValue(name, out var value) && value != "" ? value : null;
    }
}
