using System.Net;
using System.Net.Mail;
using Deed.Application.Abstractions.Services;
using Deed.Application.Abstractions.Settings;
using Microsoft.Extensions.Options;
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
            ShouldHandle = new PredicateBuilder().Handle<SmtpException>(),
            OnRetry = args =>
            {
                Log.Warning("SMTP retry attempt {Attempt} after {Delay}s", args.AttemptNumber + 1,
                    args.RetryDelay.TotalSeconds);
                return ValueTask.CompletedTask;
            }
        })
        .Build();

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        await RetryPipeline.ExecuteAsync(async token =>
        {
            SmtpSettings smtp = settings.Value;

            using SmtpClient client = new(smtp.Host, smtp.Port)
            {
                Credentials = new NetworkCredential(smtp.Username, smtp.Password),
                EnableSsl = true
            };

            using MailMessage message = new(smtp.FromAddress, to, subject, htmlBody)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message, token).ConfigureAwait(false);
            Log.Information("Email sent to {To}: {Subject}", to, subject);
        }, ct).ConfigureAwait(false);
    }
}
