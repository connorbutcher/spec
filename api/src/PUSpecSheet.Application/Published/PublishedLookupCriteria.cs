using PUSpecSheet.Application.Common;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// What a caller is looking for: the value typed into a cell that has a lookup key, optionally only on the
/// sheets of one phase or one sheet type.
/// </summary>
public sealed record PublishedLookupCriteria
{
    private const int MaxValueLength = 4000;

    private PublishedLookupCriteria(string key, string value, string? phaseCode, int? sheetTypeId)
    {
        Key = key;
        Value = value;
        PhaseCode = phaseCode;
        SheetTypeId = sheetTypeId;
    }

    public string Key { get; }

    public string Value { get; }

    public string? PhaseCode { get; }

    public int? SheetTypeId { get; }

    public static PublishedLookupCriteria Create(string? key, string? value, string? phaseCode = null, int? sheetTypeId = null)
    {
        var trimmedKey = key?.Trim() ?? string.Empty;
        if (!LookupKeyRules.IsValid(trimmedKey))
        {
            throw new InvalidRequestException($"\"{trimmedKey}\" is not a lookup key. {LookupKeyRules.Description}");
        }

        var trimmedValue = value?.Trim() ?? string.Empty;
        if (trimmedValue.Length is 0 or > MaxValueLength)
        {
            throw new InvalidRequestException($"The value to look up must be between 1 and {MaxValueLength} characters.");
        }

        return new PublishedLookupCriteria(
            trimmedKey,
            trimmedValue,
            string.IsNullOrWhiteSpace(phaseCode) ? null : phaseCode.Trim(),
            sheetTypeId);
    }
}
