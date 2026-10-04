using System.Security.Cryptography;
using System.Text;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// What a caller wants from a published sheet: the tables, sections, rows and cells to read (all of them
/// when nothing is named), the response shape, and whether to send names and captions.
/// </summary>
public sealed class PublishedSheetSelection
{
    private PublishedSheetSelection(
        IReadOnlySet<Guid> tables,
        IReadOnlySet<Guid> sections,
        IReadOnlySet<Guid> rows,
        IReadOnlySet<Guid> cells,
        PublishedSheetShape shape,
        bool includeLabels)
    {
        Tables = tables;
        Sections = sections;
        Rows = rows;
        Cells = cells;
        Shape = shape;
        IncludeLabels = includeLabels;
        Key = BuildKey();
    }

    public IReadOnlySet<Guid> Tables { get; }

    public IReadOnlySet<Guid> Sections { get; }

    public IReadOnlySet<Guid> Rows { get; }

    public IReadOnlySet<Guid> Cells { get; }

    public PublishedSheetShape Shape { get; }

    public bool IncludeLabels { get; }

    /// <summary>True when nothing is named, so the whole sheet is read.</summary>
    public bool IsEverything => Tables.Count + Sections.Count + Rows.Count + Cells.Count == 0;

    /// <summary>The same for the same selection whatever order the identifiers were given in.</summary>
    public string Key { get; }

    public static PublishedSheetSelection From(PublishedSheetQueryRequest request)
    {
        return new PublishedSheetSelection(
            ToSet(request.Tables),
            ToSet(request.Sections),
            ToSet(request.Rows),
            ToSet(request.Cells),
            request.Shape,
            request.IncludeLabels);
    }

    /// <summary>Builds a selection from query string values, each a comma-separated list of identifiers.</summary>
    public static PublishedSheetSelection Parse(
        string? tables,
        string? sections,
        string? rows,
        string? cells,
        PublishedSheetShape shape,
        string? include)
    {
        return new PublishedSheetSelection(
            ParseIds(tables, "tables"),
            ParseIds(sections, "sections"),
            ParseIds(rows, "rows"),
            ParseIds(cells, "cells"),
            shape,
            ParseInclude(include));
    }

    private static HashSet<Guid> ToSet(IReadOnlyList<Guid>? ids)
    {
        return ids is null ? [] : [.. ids];
    }

    private static HashSet<Guid> ParseIds(string? value, string name)
    {
        var result = new HashSet<Guid>();
        if (string.IsNullOrWhiteSpace(value))
        {
            return result;
        }

        foreach (var part in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!Guid.TryParse(part, out var id))
            {
                throw new InvalidRequestException($"\"{part}\" in {name} is not an identifier.");
            }

            result.Add(id);
        }

        return result;
    }

    private static bool ParseInclude(string? include)
    {
        if (string.IsNullOrWhiteSpace(include))
        {
            return false;
        }

        if (string.Equals(include.Trim(), "labels", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        throw new InvalidRequestException("The only thing that can be included is labels.");
    }

    private static void Append(StringBuilder text, char kind, IReadOnlySet<Guid> ids)
    {
        text.Append(kind);
        foreach (var id in ids.Order())
        {
            text.Append(id.ToString("N"));
        }
    }

    private string BuildKey()
    {
        var prefix = (Shape == PublishedSheetShape.Flat ? "f" : "t") + (IncludeLabels ? "l" : string.Empty);
        if (IsEverything)
        {
            return prefix + "-all";
        }

        var text = new StringBuilder();
        Append(text, 't', Tables);
        Append(text, 's', Sections);
        Append(text, 'r', Rows);
        Append(text, 'c', Cells);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString()));
        return prefix + "-" + Convert.ToHexStringLower(hash.AsSpan(0, 8));
    }
}
