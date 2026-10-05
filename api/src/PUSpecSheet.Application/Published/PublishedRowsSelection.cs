using System.Security.Cryptography;
using System.Text;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Which rows a caller wants (all of them when none is named) and which sets of repeated columns to keep
/// (all of them when none is named).
/// </summary>
public sealed class PublishedRowsSelection
{
    private PublishedRowsSelection(IReadOnlySet<Guid> rows, IReadOnlySet<string> columns)
    {
        Rows = rows;
        Columns = columns;
        Key = BuildKey();
    }

    public IReadOnlySet<Guid> Rows { get; }

    /// <summary>Names of sets of repeated columns, compared without regard to case.</summary>
    public IReadOnlySet<string> Columns { get; }

    public bool IsEverything => Rows.Count == 0;

    /// <summary>The same for the same selection whatever order it was given in.</summary>
    public string Key { get; }

    public static PublishedRowsSelection From(PublishedRowsQueryRequest request)
    {
        return new PublishedRowsSelection(
            request.Rows is null ? new HashSet<Guid>() : [.. request.Rows],
            Names(request.Columns ?? []));
    }

    /// <summary>Builds a selection from query string values, each a comma-separated list.</summary>
    public static PublishedRowsSelection Parse(string? rows, string? columns)
    {
        var ids = new HashSet<Guid>();
        foreach (var part in Split(rows))
        {
            if (!Guid.TryParse(part, out var id))
            {
                throw new InvalidRequestException($"\"{part}\" in ids is not an identifier.");
            }

            ids.Add(id);
        }

        return new PublishedRowsSelection(ids, Names(Split(columns)));
    }

    /// <summary>One row, as a selection.</summary>
    public static PublishedRowsSelection One(Guid row, string? columns)
    {
        return new PublishedRowsSelection(new HashSet<Guid> { row }, Names(Split(columns)));
    }

    private static string[] Split(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static HashSet<string> Names(IEnumerable<string> names)
    {
        return new HashSet<string>(
            names.Select(name => name.Trim()).Where(name => name.Length > 0),
            StringComparer.OrdinalIgnoreCase);
    }

    private string BuildKey()
    {
        if (IsEverything && Columns.Count == 0)
        {
            return "rows-all";
        }

        var text = new StringBuilder();
        foreach (var id in Rows.Order())
        {
            text.Append(id.ToString("N"));
        }

        foreach (var column in Columns.Select(column => column.ToUpperInvariant()).Order(StringComparer.Ordinal))
        {
            text.Append('\n').Append(column);
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString()));
        return "rows-" + Convert.ToHexStringLower(hash.AsSpan(0, 8));
    }
}
