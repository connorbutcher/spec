using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Sheets.Collaboration;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration;

// After a change to a sheet, tells the other people who have it open, but only when the change is one they can see: a
// row checked out or released, or a new version. Typing into a row that is already checked out changes nothing for
// anyone else, so it is not announced. When only rows changed hands, the announcement names them, so the others read
// those rows and not the whole sheet.
public sealed class SheetChangeAnnouncer
{
    private readonly Lock _gate = new();
    private readonly Dictionary<int, IReadOnlyList<string>> _lastSignatures = [];

    // The signature each sheet had when the others were last told about it. Rows are named against this one, not the
    // last seen, so a change a read took note of without telling anyone is still named.
    private readonly Dictionary<int, IReadOnlyList<string>> _lastAnnounced = [];

    private readonly ISheetLiveNotifier _notifier;

    public SheetChangeAnnouncer(ISheetLiveNotifier notifier)
    {
        _notifier = notifier;
    }

    public async Task AnnounceAsync(Sheet sheet, string? exceptConnectionId, CancellationToken cancellationToken)
    {
        if (!sheet.IsLive || Announce(sheet.Id, SheetLockSignature.Entries(sheet)) is not { } notice)
        {
            return;
        }

        await _notifier.SheetChangedAsync(sheet.Id, notice, exceptConnectionId, cancellationToken);
    }

    // Takes note of a live sheet that was only read. Reading can quietly drop the reader's drafts that make no
    // difference, so what was last seen has to keep up or the next real change would look like no change at all.
    // Nothing is announced: a read that announced could set two readers off refreshing each other.
    public void Observe(Sheet sheet)
    {
        if (sheet.IsLive)
        {
            var signature = SheetLockSignature.Entries(sheet);
            lock (_gate)
            {
                Changed(sheet.Id, signature);

                // The first sight of a sheet is what its first readers have; later reads aren't announced.
                _lastAnnounced.TryAdd(sheet.Id, signature);
            }
        }
    }

    private SheetChangedNotice? Announce(int sheetId, IReadOnlyList<string> signature)
    {
        lock (_gate)
        {
            if (!Changed(sheetId, signature))
            {
                return null;
            }

            var rows = _lastAnnounced.TryGetValue(sheetId, out var announced)
                ? SheetLockSignature.ChangedRows(announced, signature)
                : null;
            _lastAnnounced[sheetId] = signature;
            return rows is null ? SheetChangedNotice.Everything : new SheetChangedNotice { RowIds = rows };
        }
    }

    private bool Changed(int sheetId, IReadOnlyList<string> signature)
    {
        if (_lastSignatures.TryGetValue(sheetId, out var last) && last.SequenceEqual(signature, StringComparer.Ordinal))
        {
            return false;
        }

        _lastSignatures[sheetId] = signature;
        return true;
    }
}
