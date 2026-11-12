namespace PUSpecSheet.Contracts.Sheets;

/// <summary>Adds a table built from the latest version of a table template of the sheet's type.</summary>
public sealed record AddSheetTableRequest
{
    public AddSheetTableRequest(int tableTemplateId)
    {
        this.TableTemplateId = tableTemplateId;
    }

    public int TableTemplateId { get; init; }
}
