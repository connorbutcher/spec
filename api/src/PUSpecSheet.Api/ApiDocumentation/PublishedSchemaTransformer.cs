using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>
/// Gives the published sheet types a worked example, so the documentation shows a realistic request and
/// answer in place of placeholder values, and describes a cell's value, which can be a number, text, a
/// boolean or a date and so has no schema of its own.
/// </summary>
public sealed class PublishedSchemaTransformer : IOpenApiSchemaTransformer
{
    private const string Sheet = "5b0f7c1e-52b5-4d0a-9d55-1c1f7b6c2a10";
    private const string Table = "0c8f3c53-8f0e-4f8b-b0b3-0a8d6a2f4e21";
    private const string Section = "a4b0a3de-1f8e-4c0c-8a57-93b1f6f0c7d2";
    private const string Row = "f2a1c6b4-7d3e-4b59-9c1a-6e5d4c3b2a19";
    private const string MinimumCell = "3e9d2c7a-4b6f-4d1e-8a2c-5f7b9d1e3c40";
    private const string MaximumCell = "9a7b5c3d-1e2f-4a6b-8c0d-2e4f6a8b0c51";
    private const string UnitCell = "6c4e2a08-9b7d-4f5e-a3c1-0d2f4b6e8a62";

    private const string ValueProperty = "value";
    private const string ValueDescription =
        "The cell's value: a number, text, `true` or `false`, or a date as `yyyy-MM-dd`. A dropdown gives the "
        + "chosen option's text, or its number for a number dropdown.";

    private static readonly Dictionary<Type, Func<JsonNode>> Examples = new()
    {
        [typeof(PublishedSheetReferenceDto)] = Reference,
        [typeof(PublishedVersionDto)] = Version,
        [typeof(PublishedSheetQueryRequest)] = QueryRequest,
        [typeof(PublishedSheetDto)] = SheetAsTree,
        [typeof(PublishedRowsDto)] = Rows,
        [typeof(PublishedSheetRowDto)] = OneRow,
        [typeof(PublishedRowsQueryRequest)] = RowsRequest,
        [typeof(PublishedLookupDto)] = Lookup,
    };

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        // Only the type itself gets the example, not a property or list item that happens to be of it.
        if (context.JsonPropertyInfo is null && Examples.TryGetValue(context.JsonTypeInfo.Type, out var example))
        {
            schema.Examples = [example()];
        }

        if (context.JsonTypeInfo.Type == typeof(PublishedCellDto)
            && schema.Properties?.TryGetValue(ValueProperty, out var value) == true
            && value is OpenApiSchema valueSchema)
        {
            valueSchema.Description = ValueDescription;
        }

        return Task.CompletedTask;
    }

    private static JsonObject Reference()
    {
        return new JsonObject
        {
            ["sheet"] = Sheet,
            ["phase"] = "V6",
            ["sheetType"] = 1,
            ["latestVersion"] = 3,
        };
    }

    private static JsonObject Version()
    {
        return new JsonObject
        {
            ["version"] = 3,
            ["publishedAtUtc"] = "2026-09-30T14:02:11Z",
            ["note"] = "Raised the oil pressure maximum",
        };
    }

    private static JsonObject QueryRequest()
    {
        return new JsonObject
        {
            ["sections"] = new JsonArray(Section),
            ["cells"] = new JsonArray(MinimumCell, MaximumCell),
            ["shape"] = "Tree",
            ["includeLabels"] = true,
        };
    }

    private static JsonObject SheetAsTree()
    {
        var cells = new JsonArray(
            Cell(MinimumCell, 2.5, "Min"),
            Cell(MaximumCell, 4.2, "Max"),
            Cell(UnitCell, "bar", "Unit"));

        var section = new JsonObject
        {
            ["id"] = Section,
            ["name"] = "Oil system",
            ["rows"] = new JsonArray(new JsonObject { ["id"] = Row, ["cells"] = cells }),
            ["sections"] = new JsonArray(),
        };

        var table = new JsonObject
        {
            ["id"] = Table,
            ["title"] = "Pressures",
            ["sections"] = new JsonArray(section),
        };

        return new JsonObject
        {
            ["sheet"] = Sheet,
            ["version"] = 3,
            ["publishedAtUtc"] = "2026-09-30T14:02:11Z",
            ["tables"] = new JsonArray(table),
            ["missing"] = new JsonArray(),
        };
    }

    private static JsonObject Rows()
    {
        return new JsonObject
        {
            ["sheet"] = Sheet,
            ["version"] = 3,
            ["publishedAtUtc"] = "2026-09-30T14:02:11Z",
            ["rows"] = new JsonObject
            {
                [Row] = new JsonObject
                {
                    ["section"] = "Limits",
                    ["values"] = new JsonArray("Bore (mm)"),
                    ["columns"] = new JsonObject
                    {
                        ["P-1003"] = new JsonArray(82.0, 82.04),
                        ["P-1004"] = new JsonArray(82.01, 82.05),
                    },
                },
            },
            ["missing"] = new JsonArray(),
        };
    }

    private static JsonObject OneRow()
    {
        return new JsonObject
        {
            ["sheet"] = Sheet,
            ["version"] = 3,
            ["publishedAtUtc"] = "2026-09-30T14:02:11Z",
            ["row"] = Row,
            ["section"] = "Limits",
            ["values"] = new JsonArray("Bore (mm)"),
            ["columns"] = new JsonObject { ["P-1003"] = new JsonArray(82.0, 82.04) },
        };
    }

    private static JsonObject RowsRequest()
    {
        return new JsonObject
        {
            ["rows"] = new JsonArray(Row),
            ["columns"] = new JsonArray("P-1003", "P-1004"),
        };
    }

    private static JsonObject Lookup()
    {
        var match = new JsonObject
        {
            ["sheet"] = Sheet,
            ["phase"] = "V6",
            ["sheetType"] = 2,
            ["version"] = 3,
            ["publishedAtUtc"] = "2026-09-30T14:02:11Z",
            ["table"] = Table,
            ["title"] = "Piston parts",
            ["column"] = "P-1003",
            ["headings"] = new JsonArray("Min", "Max"),
            ["rows"] = new JsonObject
            {
                [Row] = new JsonObject
                {
                    ["section"] = "Limits",
                    ["values"] = new JsonArray("Bore (mm)"),
                    ["columns"] = new JsonObject { ["P-1003"] = new JsonArray(82.0, 82.04) },
                },
            },
        };

        return new JsonObject
        {
            ["key"] = "partNumber",
            ["value"] = "P-1003",
            ["matches"] = new JsonArray(match),
        };
    }

    private static JsonObject Cell(string id, JsonNode value, string caption)
    {
        return new JsonObject
        {
            ["id"] = id,
            ["value"] = value,
            ["caption"] = caption,
        };
    }
}
