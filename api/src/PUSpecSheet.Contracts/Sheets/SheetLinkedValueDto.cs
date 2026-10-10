namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// One value of a row a linked dropdown can pick, as text, with what to call it: its column, and for a
/// table with repeated columns the name at the top of its set, such as "P-1003 Min".
/// </summary>
public sealed record SheetLinkedValueDto(string Label, string Value);
