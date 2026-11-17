using System.Security.Cryptography;
using System.Text;
using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Common;

namespace PUSpecSheet.DataAccess.Published.Rows;

public sealed class PublishedRowsSelection
{
    private PublishedRowsSelection(IReadOnlySet<Guid> rows, IReadOnlySet<string> columns)
    {
        Rows = rows;
        Columns = columns;
        Key = BuildKey();
    }

    public IReadOnlySet<Guid> Rows { get; }

    public IReadOnlySet<string> Columns { get; }

    public bool IsEverything => Rows.Count == 0;

    public string Key { get; }

    public static PublishedRowsSelection From(PublishedRowsQueryRequest request)
    {
        return new PublishedRowsSelection(
            request.Rows is null ? new HashSet<Guid>() : [.. request.Rows],
            Names(request.Columns ?? []));
    }

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
