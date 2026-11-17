using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>Sets a table's title. An empty title falls back to the template's name.</summary>
public sealed record SetSheetTableTitleRequest
{
    public SetSheetTableTitleRequest([StringLength(200)] string? title)
    {
        this.Title = title;
    }

    public string? Title { get; init; }
}
