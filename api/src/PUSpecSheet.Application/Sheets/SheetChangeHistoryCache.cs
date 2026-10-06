using PUSpecSheet.Application.Common;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Keeps the change history of live sheets, keyed by sheet and its latest published version. A history is
/// worked out from every published revision and value the sheet has ever had, so it costs more with each
/// version, yet it only changes when a new version is published; every read and edit in between reuses it.
/// </summary>
public sealed class SheetChangeHistoryCache() : SizedMemoryCache<SheetChangeHistory>(MaximumChanges)
{
    private const long MaximumChanges = 1_000_000;
}
