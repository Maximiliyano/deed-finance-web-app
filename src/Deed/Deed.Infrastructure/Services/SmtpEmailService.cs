using System.Net.Sockets;
using Deed.Application.Abstractions.Services;
using Deed.Application.Abstractions.Settings;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;
using Polly.Retry;
using Serilog;

namespace Deed.Infrastructure.Services;

internal sealed class SmtpEmailService(IOptions<SmtpSettings> settings) : IEmailService
{
    private static readonly ResiliencePipeline RetryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(2),
            BackoffType = DelayBackoffType.Exponential,
            ShouldHandle = new PredicateBuilder()
                .Handle<IOException>()
                .Handle<SocketException>()
                .Handle<TimeoutException>()
                .Handle<SmtpProtocolException>()
                .Handle<SmtpCommandException>(e => (int)e.StatusCode is >= 400 and <= 500),
            OnRetry = args =>
            {
                Log.Warning(args.Outcome.Exception, "SMTP retry attempt {Attempt} after {Delay}s", args.AttemptNumber + 1,
                    args.RetryDelay.TotalSeconds);
                return ValueTask.CompletedTask;
            }
        })
        .Build();

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        SmtpSettings smtp = settings.Value;

        if (string.IsNullOrEmpty(smtp.Host) || string.IsNullOrEmpty(smtp.FromAddress))
        {
            throw new InvalidOperationException("SmtpSettings Host and FromAddress must be configured.");
        }

        using MimeMessage message = new();
        
        message.From.Add(MailboxAddress.Parse(smtp.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();
        
        await RetryPipeline.ExecuteAsync(async token =>
        {
            using SmtpClient client = new() { Timeout = 30_000 };

            await client.ConnectAsync(smtp.Host, smtp.Port, SecureSocketOptions.Auto, token).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(smtp.Username))
            {
                await client.AuthenticateAsync(smtp.Username, smtp.Password, token).ConfigureAwait(false);
            }
            
            await client.SendAsync(message, token).ConfigureAwait(false);
            await client.DisconnectAsync(true, token).ConfigureAwait(false);
            
            Log.Information("Email sent to {To}: {Subject}", to, subject);
        }, ct).ConfigureAwait(false);
    }
}
