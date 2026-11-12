using PUSpecSheet.Application.Common;

namespace PUSpecSheet.Application.Sheets.History;

/// <summary>
/// Keeps the change history of sheets, keyed by sheet and the version viewed (the newest for the live view).
/// A history is worked out from every published revision and value the sheet had up to that version, so it
/// costs more with each version, yet it never changes for a version once published; every read and edit
/// until the next version, and every look back at an earlier one, reuses it.
/// </summary>
public sealed class SheetChangeHistoryCache : SizedMemoryCache<SheetChangeHistory>
{
    private const long MaximumChanges = 1_000_000;

    public SheetChangeHistoryCache()
        : base(MaximumChanges)
    {
    }
}
