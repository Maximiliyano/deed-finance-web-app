using Deed.Application.Abstractions.Services;
using Deed.Application.Abstractions.Settings;
using Deed.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Deed.Infrastructure.Services;

internal sealed class KeyRingProtectionService(
    IServiceScopeFactory scopeFactory,
    IKeyManager keyManager,
    ITokenProtector tokenProtector,
    IOptions<DataProtectionSettings> options)
    : IHostedService
{
    private const int KeyLifetimeDays = 90;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;

        if (!settings.HasCertificate)
        {
            Log.Warning("The data protection key ring is stored unprotected next to the encrypted bank tokens. Set DataProtectionSettings:CertificateBase64 to protect it.");
            return;
        }

        if (!settings.ReprotectExistingKeys)
        {
            return;
        }

        try
        {
            await ReprotectAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Error(ex, "Re-protecting the data protection key ring failed. No key was deleted.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task ReprotectAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DeedDbContext>();

        var plaintext = (await db.DataProtectionKeys.ToListAsync(cancellationToken).ConfigureAwait(false))
            .Where(key => DataProtectionKeyXml.IsPlaintextKey(key.Xml))
            .ToList();

        if (plaintext.Count == 0)
        {
            Log.Information("The data protection key ring is already protected.");
            return;
        }

        var now = DateTimeOffset.UtcNow;

        keyManager.CreateNewKey(now, now.AddDays(KeyLifetimeDays));

        var plaintextIds = plaintext.Select(key => DataProtectionKeyXml.KeyId(key.Xml)).OfType<Guid>().ToHashSet();
        var newest = keyManager.GetAllKeys().Where(key => !key.IsRevoked).OrderByDescending(key => key.ActivationDate).First();

        if (plaintextIds.Contains(newest.KeyId))
        {
            Log.Error("The new data protection key was not picked as the default. No key was deleted.");
            return;
        }

        var connections = await db.BankConnections.IgnoreQueryFilters().ToListAsync(cancellationToken).ConfigureAwait(false);
        var moved = 0;
        var unreadable = 0;

        foreach (var connection in connections)
        {
            if (tokenProtector.Unprotect(connection.EncryptedToken) is not { } secret)
            {
                unreadable++;
                continue;
            }

            connection.EncryptedToken = tokenProtector.Protect(secret);
            moved++;
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        db.DataProtectionKeys.RemoveRange(plaintext);

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Warning(
            "Data protection key ring re-protected: {Moved} bank token(s) re-encrypted, {Unreadable} were already unreadable, {Keys} plaintext key(s) deleted. Signed-in users must sign in again.",
            moved,
            unreadable,
            plaintext.Count);
    }
}
