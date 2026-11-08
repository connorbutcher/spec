namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>
/// What changed on a sheet someone has open. When only rows were checked out or released, they are listed
/// and only those need reading again; otherwise (a version, or a table, section or column block) the list
/// is null and the whole sheet does.
/// </summary>
public sealed record SheetChangedNoticeDto(IReadOnlyList<int>? RowIds)
{
    public static SheetChangedNoticeDto Everything { get; } = new((IReadOnlyList<int>?)null);
}
