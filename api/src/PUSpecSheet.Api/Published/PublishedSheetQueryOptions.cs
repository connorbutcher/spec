using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Published;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Api.Published;

/// <summary>
/// The query string of a published sheet read. <c>tables</c>, <c>sections</c>, <c>rows</c> and
/// <c>cells</c> are comma-separated identifiers, combined; with none of them the whole sheet is returned.
/// </summary>
public sealed class PublishedSheetQueryOptions
{
    [FromQuery(Name = "tables")]
    public string? Tables { get; set; }

    [FromQuery(Name = "sections")]
    public string? Sections { get; set; }

    [FromQuery(Name = "rows")]
    public string? Rows { get; set; }

    [FromQuery(Name = "cells")]
    public string? Cells { get; set; }

    /// <summary><c>tree</c> (the default) or <c>flat</c>.</summary>
    [FromQuery(Name = "shape")]
    public PublishedSheetShape Shape { get; set; } = PublishedSheetShape.Tree;

    /// <summary><c>labels</c> adds table titles, section names and cell captions.</summary>
    [FromQuery(Name = "include")]
    public string? Include { get; set; }

    public PublishedSheetSelection ToSelection()
    {
        return PublishedSheetSelection.Parse(Tables, Sections, Rows, Cells, Shape, Include);
    }
}
