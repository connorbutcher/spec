namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>
/// A row someone has clicked into but not changed yet. It is theirs while they stay in it; nothing is
/// saved, so it is not part of the sheet itself and is sent alongside it. Once they change the row, the
/// sheet shows it locked to them as usual.
/// </summary>
public sealed record RowCheckoutDto
{
    public RowCheckoutDto(int rowId, int userId, string userName)
    {
        this.RowId = rowId;
        this.UserId = userId;
        this.UserName = userName;
    }

    public int RowId { get; init; }

    public int UserId { get; init; }

    public string UserName { get; init; }
}
