using System.Globalization;
using System.Text.Json;
using PUSpecSheet.Abstractions.Domain.Properties.Cell;

namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// What one cell holds in one copy or revision: its values and any configuration override of its own.
/// <see cref="Signature"/> puts it all in one string, so two contents are compared as text.
/// </summary>
internal sealed record CellContent
{
    public static readonly CellContent Empty = new()
    {
        Text = null,
        Number = null,
        PairX = null,
        PairY = null,
        Linked = null,
        ConfigurationOverride = null
    };

    public required string? Text { get; init; }

    public required double? Number { get; init; }

    public required string? PairX { get; init; }

    public required string? PairY { get; init; }

    public required LinkedSheetTarget? Linked { get; init; }

    public required ICellProperties? ConfigurationOverride { get; init; }

    public bool HasPair => PairX is not null || PairY is not null;

    public bool IsEmpty => Text is null && Number is null && !HasPair && Linked is null && ConfigurationOverride is null;

    public string Signature => JsonSerializer.Serialize(new object?[]
    {
        Text,
        Number?.ToString("R", CultureInfo.InvariantCulture),
        HasPair ? new[] { PairX, PairY } : null,
        Linked is null ? null : new object?[] { Linked.SheetTypeId, Linked.PhaseId, Linked.TableReferenceId, Linked.ItemReferenceId },
        SnapshotJson.Of(ConfigurationOverride)
    });
}
