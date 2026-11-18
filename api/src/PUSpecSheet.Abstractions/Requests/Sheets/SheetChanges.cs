using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Abstractions.Operations;

namespace PUSpecSheet.Abstractions.Requests.Sheets;

/// <summary>
/// Changes to a sheet's draft. The operations are the ones the spec sheet edits use, with the ids of the sheet's items
/// (see <c>Sheet</c>) in place of a copy's.
/// </summary>
public sealed record SheetChanges
{
    [MinLength(1)]
    public required IChangeOp[] Operations { get; init; }
}
