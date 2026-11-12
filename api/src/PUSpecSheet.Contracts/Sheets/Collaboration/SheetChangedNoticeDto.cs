namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>
/// What changed on a sheet someone has open. When only rows were checked out or released, they are listed
/// and only those need reading again; otherwise (a version, or a table, section or column block) the list
/// is null and the whole sheet does.
/// </summary>
public sealed record SheetChangedNoticeDto
{
    public SheetChangedNoticeDto(IReadOnlyList<int>? rowIds)
    {
        this.RowIds = rowIds;
    }

    public IReadOnlyList<int>? RowIds { get; init; }

    public static SheetChangedNoticeDto Everything { get; } = new((IReadOnlyList<int>?)null);
}
