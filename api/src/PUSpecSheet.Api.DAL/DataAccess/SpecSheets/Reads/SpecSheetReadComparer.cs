using System.Text.Json;
using System.Text.Json.Nodes;
using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;

/// <summary>
/// Compares a spec sheet read from its copy with the same sheet read from the revisions. Both are written out as
/// "path = value" lines, with items found by their ReferenceID rather than where they come in a list, and the lines
/// each has that the other hasn't are the difference. Left out: row ids, which differ between the two by design, the
/// group a group is linked to is compared by its ReferenceID instead, and who created and last changed each table,
/// group, row and cell, which the copies keep per item and the revisions per revision.
/// </summary>
internal static class SpecSheetReadComparer
{
    private const int MostLinesShown = 20;

    private static readonly HashSet<string> _ids =
    [
        "Id",
        "SpecSheetId",
        "SpecSheetTableId",
        "SpecSheetTableGroupId",
        "SpecSheetTableRowId",
        "SpecSheetTableCellId",
        "SpecSheetTableWidgetId",
        "SpecSheetTableGroupWidgetId",
        "WorkflowStepId"
    ];

    private static readonly HashSet<string> _items = ["Tables", "Groups", "Rows", "Cells"];

    private static readonly HashSet<string> _itemAudit =
    [
        "CreatedByUserId",
        "CreatedDateTime",
        "CreatedByUser",
        "LastModifiedByUserId",
        "LastModifiedDateTime",
        "LastModifiedByUser"
    ];

    /// <summary>
    /// Null when the two read the same; otherwise what each has that the other hasn't, for a person to read.
    /// </summary>
    public static string? Difference(SpecSheet fromCopy, SpecSheet fromRevisions)
    {
        var copyLines = Lines(fromCopy);
        var revisionLines = Lines(fromRevisions);
        var onlyInCopy = copyLines.Except(revisionLines, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        var onlyInRevisions = revisionLines.Except(copyLines, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        if (onlyInCopy.Count == 0 && onlyInRevisions.Count == 0)
        {
            return null;
        }

        return $"only in the copy ({onlyInCopy.Count}):\n{Shown(onlyInCopy)}\nonly in the revisions ({onlyInRevisions.Count}):\n{Shown(onlyInRevisions)}";
    }

    private static string Shown(List<string> lines)
    {
        var shown = lines.Take(MostLinesShown).Select(line => $"  {line}");
        return string.Join('\n', lines.Count > MostLinesShown ? shown.Append($"  ... and {lines.Count - MostLinesShown} more") : shown);
    }

    private static HashSet<string> Lines(SpecSheet specSheet)
    {
        var node = JsonSerializer.SerializeToNode(specSheet)
            ?? throw new InvalidOperationException("A spec sheet can't be written out as JSON.");
        LinkGroupsByReference(node);

        var lines = new HashSet<string>(StringComparer.Ordinal);
        Flatten(node, "sheet", parentName: null, lines);
        return lines;
    }

    // A group's LinkedSpecSheetTableGroupId is the id of the group it is nested under; the ids differ between the two
    // reads, so it is replaced by that group's ReferenceID.
    private static void LinkGroupsByReference(JsonNode sheet)
    {
        var groups = (sheet["Tables"] as JsonArray ?? new JsonArray())
            .SelectMany(table => table?["Groups"] as JsonArray ?? new JsonArray())
            .OfType<JsonObject>()
            .ToList();
        var references = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var group in groups)
        {
            if (group["Id"] is { } id && group["ReferenceId"] is { } reference)
            {
                references[id.ToJsonString()] = reference.ToJsonString();
            }
        }

        foreach (var group in groups)
        {
            if (group["LinkedSpecSheetTableGroupId"] is { } linked)
            {
                group["LinkedSpecSheetTableGroupId"] = references.TryGetValue(linked.ToJsonString(), out var reference)
                    ? JsonNode.Parse(reference)
                    : (JsonNode?)"(a group not on the sheet)";
            }
        }
    }

    private static void Flatten(JsonNode? node, string path, string? parentName, HashSet<string> lines)
    {
        switch (node)
        {
            case JsonObject item:
                var properties = item
                    .Where(property => !_ids.Contains(property.Key))
                    .Where(property => parentName is null || !_items.Contains(parentName) || !_itemAudit.Contains(property.Key))
                    .OrderBy(property => property.Key, StringComparer.Ordinal)
                    .ToList();
                if (properties.Count == 0)
                {
                    lines.Add($"{path} = {{}}");
                }

                foreach (var (name, value) in properties)
                {
                    Flatten(value, $"{path}.{name}", name, lines);
                }

                break;

            case JsonArray list:
                if (list.Count == 0)
                {
                    lines.Add($"{path} = []");
                }

                // Items are found by their ReferenceID; anything else by what it holds, so a list's order doesn't matter.
                var seen = new Dictionary<string, int>(StringComparer.Ordinal);
                var keyed = list
                    .Select(element => (Key: element is JsonObject withReference && withReference["ReferenceId"] is { } reference
                        ? reference.GetValue<string>()
                        : Without(element, parentName), Element: element))
                    .OrderBy(keyedElement => keyedElement.Key, StringComparer.Ordinal);
                foreach (var (key, element) in keyed)
                {
                    seen[key] = seen.GetValueOrDefault(key) + 1;
                    var segment = seen[key] == 1 ? key : $"{key}#{seen[key]}";
                    Flatten(element, $"{path}[{segment}]", parentName, lines);
                }

                break;

            default:
                lines.Add($"{path} = {node?.ToJsonString() ?? "null"}");
                break;
        }
    }

    // What an element without a ReferenceID holds, less its ids, to tell it apart from its siblings by.
    private static string Without(JsonNode? element, string? parentName)
    {
        var lines = new HashSet<string>(StringComparer.Ordinal);
        Flatten(element, "", parentName, lines);
        return string.Join(", ", lines.Order(StringComparer.Ordinal));
    }
}
