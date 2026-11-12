using System.Text.Json.Serialization;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// Some rows of a live sheet as the viewer now sees them, with the parts of the sheet that follow from
/// those rows: the viewer's draft count, other people's drafts, the choices of linked dropdowns and the
/// rows of their tables that can be linked to. It is what saving a row answers with, and what is read
/// when someone else has checked rows out or released them, in place of the whole sheet.
/// </summary>
public sealed record SheetRowsChangeDto
{
    public SheetRowsChangeDto(int sheetId, IReadOnlyList<SheetRowDto> rows, int? latestVersionNumber, int myDraftCount)
    {
        this.SheetId = sheetId;
        this.Rows = rows;
        this.LatestVersionNumber = latestVersionNumber;
        this.MyDraftCount = myDraftCount;
    }

    /// <summary>The sheet's id.</summary>
    public int SheetId { get; init; }

    /// <summary>The rows asked for that are on the sheet. A row asked for and not listed has gone.</summary>
    public IReadOnlyList<SheetRowDto> Rows { get; init; }

    /// <summary>The newest version of the sheet, so a reader can tell when it has to read everything.</summary>
    public int? LatestVersionNumber { get; init; }

    /// <summary>How many of the viewer's own changes are waiting to be published.</summary>
    public int MyDraftCount { get; init; }

    /// <summary>Other people's unpublished changes on the sheet, by person.</summary>
    public IReadOnlyList<SheetDraftSummaryDto> OtherDrafts { get; init; } = [];

    /// <summary>The choices of every column a linked dropdown on the sheet is pointed at.</summary>
    public IReadOnlyList<SheetLinkedSourceDto> LinkedSources { get; init; } = [];

    /// <summary>For each table the rows are in, its rows a linked dropdown can take its choices from.</summary>
    public IReadOnlyList<SheetTableLinkableRowsDto> LinkableRows { get; init; } = [];

    /// <summary>The template cells the rows' cells were built from, each once.</summary>
    public IReadOnlyList<TemplateCellDto> TemplateCells { get; init; } = [];

    /// <summary>
    /// The whole live sheet the rows were taken from. Never sent: the server tells the other people on the
    /// sheet about a change from it.
    /// </summary>
    [JsonIgnore]
    public SheetDto? Sheet { get; init; }
}
