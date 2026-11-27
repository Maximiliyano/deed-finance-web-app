using Deed.Application.Abstractions.Settings;
using Deed.Application.BankConnections.Statements;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Quartz;
using Serilog;

namespace Deed.Infrastructure.BackgroundJobs.StatementMailSync;

[DisallowConcurrentExecution]
public sealed class StatementMailSyncJob(
    IOptions<StatementMailSettings> options,
    IStatementMailImportService importService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var settings = options.Value;

        if (!settings.IsEnabled)
        {
            Log.Debug("Job skipped: {Name} is not configured", nameof(StatementMailSyncJob));

            return;
        }

        Log.Information("Job started: {Name}", nameof(StatementMailSyncJob));

        try
        {
            await PollAsync(settings, context.CancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Error(ex, "Statement mailbox {Host} could not be polled", settings.Host);
        }

        Log.Information("Job finished: {Name}", nameof(StatementMailSyncJob));
    }

    private async Task PollAsync(StatementMailSettings settings, CancellationToken cancellationToken)
    {
        using var client = new ImapClient();

        await client
            .ConnectAsync(settings.Host, settings.Port, settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable, cancellationToken)
            .ConfigureAwait(false);

        await client.AuthenticateAsync(settings.UserName, settings.Password, cancellationToken).ConfigureAwait(false);

        var inbox = await client.GetFolderAsync(settings.Folder, cancellationToken).ConfigureAwait(false);

        await inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken).ConfigureAwait(false);

        var unseen = await inbox.SearchAsync(SearchQuery.NotSeen, cancellationToken).ConfigureAwait(false);

        if (unseen.Count > 0)
        {
            var processed = await EnsureFolderAsync(client, settings.ProcessedFolder, cancellationToken).ConfigureAwait(false);

            foreach (var uid in unseen.Take(settings.MaxMessagesPerRun))
            {
                await ProcessAsync(inbox, processed, uid, settings, cancellationToken).ConfigureAwait(false);
            }
        }

        await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
    }

    private async Task ProcessAsync(IMailFolder inbox, IMailFolder processed, UniqueId uid, StatementMailSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var message = await inbox.GetMessageAsync(uid, cancellationToken).ConfigureAwait(false);

            var result = await importService
                .ImportAsync(StatementMailMapper.Map(message, settings.MaxAttachmentBytes), cancellationToken)
                .ConfigureAwait(false);

            Log.Information("Statement mail \"{Subject}\" handled: {Outcome}, {Created} new transaction(s)", message.Subject, result.Outcome, result.Created);

            await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, cancellationToken).ConfigureAwait(false);
            await inbox.MoveToAsync(uid, processed, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Error(ex, "Statement mail {Uid} failed and stays in the mailbox for the next run", uid);
        }
    }

    private static async Task<IMailFolder> EnsureFolderAsync(ImapClient client, string name, CancellationToken cancellationToken)
    {
        var root = client.GetFolder(client.PersonalNamespaces[0]);

        try
        {
            return await root.GetSubfolderAsync(name, cancellationToken).ConfigureAwait(false);
        }
        catch (FolderNotFoundException)
        {
            return await root.CreateAsync(name, true, cancellationToken).ConfigureAwait(false);
        }
    }
}
