using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// After a change to a sheet, tells the other people who have it open, but only when the change is one
/// they can see: a row checked out or released, or a new version. Typing into a row that is already
/// checked out changes nothing for anyone else, so it is not announced. When only rows changed hands, the
/// announcement names them, so the others read those rows and not the whole sheet.
/// </summary>
public sealed class SheetChangeAnnouncer(ISheetLiveNotifier notifier)
{
    private readonly Lock gate = new();
    private readonly Dictionary<int, IReadOnlyList<string>> lastSignatures = [];

    /// <summary>
    /// The signature each sheet had when the others were last told about it. Rows are named against this
    /// one, not the last seen, so a change a read took note of without telling anyone is still named.
    /// </summary>
    private readonly Dictionary<int, IReadOnlyList<string>> lastAnnounced = [];

    /// <param name="sheet">The live sheet as it stands after the change.</param>
    /// <param name="exceptConnectionId">The connection that made the change, which already has the result.</param>
    /// <param name="cancellationToken">Cancels the announcement.</param>
    public async Task AnnounceAsync(SheetDto sheet, string? exceptConnectionId, CancellationToken cancellationToken)
    {
        if (!sheet.IsLive || Announce(sheet.Id, SheetLockSignature.Entries(sheet)) is not { } notice)
        {
            return;
        }

        await notifier.SheetChangedAsync(sheet.Id, notice, exceptConnectionId, cancellationToken);
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
            var signature = SheetLockSignature.Entries(sheet);
            lock (gate)
            {
                Changed(sheet.Id, signature);

                // The first sight of a sheet is what its first readers have; later reads aren't announced.
                lastAnnounced.TryAdd(sheet.Id, signature);
            }
        }
    }

    /// <summary>
    /// Remembers the signature. When it differs from the last one seen (or there was none), what to tell the
    /// others: the rows that differ from what they were last told, or everything. Null when nothing changed.
    /// </summary>
    private SheetChangedNoticeDto? Announce(int sheetId, IReadOnlyList<string> signature)
    {
        lock (gate)
        {
            if (!Changed(sheetId, signature))
            {
                return null;
            }

            var rows = lastAnnounced.TryGetValue(sheetId, out var announced)
                ? SheetLockSignature.ChangedRows(announced, signature)
                : null;
            lastAnnounced[sheetId] = signature;
            return rows is null ? SheetChangedNoticeDto.Everything : new SheetChangedNoticeDto(rows);
        }
    }

    /// <summary>
    /// Remembers the signature. True when it differs from the last one, or there was no last one. Called
    /// holding <see cref="gate"/>.
    /// </summary>
    private bool Changed(int sheetId, IReadOnlyList<string> signature)
    {
        if (lastSignatures.TryGetValue(sheetId, out var last) && last.SequenceEqual(signature, StringComparer.Ordinal))
        {
            return false;
        }

        lastSignatures[sheetId] = signature;
        return true;
    }
}
