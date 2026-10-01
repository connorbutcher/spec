using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>Publishes all of the viewer's drafts on a sheet as the next version, with an optional note.</summary>
public sealed record PublishSheetRequest([StringLength(1000)] string? Note);
