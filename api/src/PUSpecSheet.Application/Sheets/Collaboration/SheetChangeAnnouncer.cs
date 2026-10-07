using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// After a change to a sheet, tells the other people who have it open, but only when the change is one
/// they can see: a row checked out or released, or a new version. Typing into a row that is already
/// checked out changes nothing for anyone else, so it is not announced.
/// </summary>
public sealed class SheetChangeAnnouncer(ISheetLiveNotifier notifier)
{
    private readonly Lock gate = new();
    private readonly Dictionary<int, string> lastSignatures = [];

    /// <param name="sheet">The live sheet as it stands after the change.</param>
    /// <param name="exceptConnectionId">The connection that made the change, which already has the result.</param>
    /// <param name="cancellationToken">Cancels the announcement.</param>
    public async Task AnnounceAsync(SheetDto sheet, string? exceptConnectionId, CancellationToken cancellationToken)
    {
        if (!sheet.IsLive || !Changed(sheet.Id, SheetLockSignature.Of(sheet)))
        {
            return;
        }

        await notifier.SheetChangedAsync(sheet.Id, exceptConnectionId, cancellationToken);
    }

    /// <summary>
    /// Takes note of a live sheet that was only read. Reading can quietly drop the reader's drafts that
    /// make no difference, so what was last seen has to keep up or the next real change would look like
    /// no change at all. Nothing is announced: a read that announced could set two readers off refreshing
    /// each other.
    /// </summary>
    public void Observe(SheetDto sheet)
    {
        if (sheet.IsLive)
        {
            Changed(sheet.Id, SheetLockSignature.Of(sheet));
        }
    }

    /// <summary>Remembers the signature. True when it differs from the last one, or there was no last one.</summary>
    private bool Changed(int sheetId, string signature)
    {
        lock (gate)
        {
            if (lastSignatures.TryGetValue(sheetId, out var last) && last == signature)
            {
                return false;
            }

            lastSignatures[sheetId] = signature;
            return true;
        }
    }
}
