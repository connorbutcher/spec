using System.Globalization;
using System.Text.Json;
using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Database.Conversions;

namespace PUSpecSheet.Migrator.Snapshots;

// What one cell holds in one copy. Signature puts it all in one string, so two contents are compared as text.
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
        CellInstanceSettings? settings = null,
        LinkedSheetTarget? linked = null)
    {
        Text = text;
        Number = number;
        Date = date;
        Boolean = boolean;
        OptionId = optionId;
        LabelledText = labelledText ?? [];
        Settings = settings;
        Linked = linked;
        Signature = BuildSignature();
    }

    public string? Text { get; }

    public decimal? Number { get; }

    public DateOnly? Date { get; }

    public bool? Boolean { get; }

    public int? OptionId { get; }

    public IReadOnlyList<LabelledSlot> LabelledText { get; }

    public CellInstanceSettings? Settings { get; }

    public LinkedSheetTarget? Linked { get; }

    public string Signature { get; }

    public bool IsEmpty => Text is null && Number is null && Date is null && Boolean is null && OptionId is null
        && LabelledText.Count == 0 && Settings is null && Linked is null;

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
            Linked is null ? null : new object?[] { Linked.SheetTypeId, Linked.PhaseId, Linked.TablePublicId, Linked.ItemPublicId },
        };

        return JsonSerializer.Serialize(parts);
    }
}
