using System.Globalization;
using System.Text.Json;
using PUSpecSheet.Database.Conversions;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// Everything a cell holds: at most one value of its kind (labelled text has one per slot) and the settings
/// chosen for it on the sheet. Two contents are the same when their <see cref="Signature"/>s are, which ignores
/// trailing zeros on numbers, the order slots were read in and how the settings JSON was laid out.
/// </summary>
internal sealed class CellContent
{
    public static readonly CellContent Empty = new();

    public CellContent(
        string? text = null,
        decimal? number = null,
        DateOnly? date = null,
        bool? boolean = null,
        int? optionId = null,
        IReadOnlyList<LabelledSlot>? labelledText = null,
        CellInstanceSettings? settings = null)
    {
        Text = text;
        Number = number;
        Date = date;
        Boolean = boolean;
        OptionId = optionId;
        LabelledText = labelledText ?? [];
        Settings = settings;
        Signature = BuildSignature();
    }

    public string? Text { get; }

    public decimal? Number { get; }

    public DateOnly? Date { get; }

    public bool? Boolean { get; }

    public int? OptionId { get; }

    public IReadOnlyList<LabelledSlot> LabelledText { get; }

    public CellInstanceSettings? Settings { get; }

    /// <summary>A canonical text form of the content, for comparing and for telling a user what differs.</summary>
    public string Signature { get; }

    public bool IsEmpty => Text is null && Number is null && Date is null && Boolean is null && OptionId is null
        && LabelledText.Count == 0 && Settings is null;

    private string BuildSignature()
    {
        // Dividing by 1.000… drops a decimal's trailing zeros, so 1.5 and 1.5000000000 read the same.
        var parts = new object?[]
        {
            Text,
            Number is { } number ? number / 1.000000000000000000000000000000000m : null,
            Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Boolean,
            OptionId,
            LabelledText
                .OrderBy(slot => slot.SlotKey, StringComparer.Ordinal)
                .Select(slot => new object[] { slot.SlotKey, slot.Position, slot.Value })
                .ToArray(),
            Settings is null ? null : CellJson.Serialize(Settings),
        };

        return JsonSerializer.Serialize(parts);
    }
}
