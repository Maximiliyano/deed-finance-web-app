using System.Net;
using System.Net.Mail;
using Deed.Application.Abstractions.Services;
using Deed.Application.Abstractions.Settings;
using Microsoft.Extensions.Options;
using Serilog;

namespace Deed.Infrastructure.Services;

internal sealed class SmtpEmailService(IOptions<SmtpSettings> settings) : IEmailService
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var smtp = settings.Value;

        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            Credentials = new NetworkCredential(smtp.Username, smtp.Password),
            EnableSsl = true
        };

        using var message = new MailMessage(smtp.FromAddress, to, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(message, ct).ConfigureAwait(false);
        Log.Information("Email sent to {To}: {Subject}", to, subject);
    }
}
