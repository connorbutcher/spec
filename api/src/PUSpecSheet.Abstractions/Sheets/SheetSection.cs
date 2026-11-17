using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>
/// A section copy on a sheet table. Its own rows come first, then its sub-sections. The template's
/// name, role and instance counts are carried along so the screen can lay it out like the template.
/// </summary>
public sealed record SheetSection
{
    public required int Id { get; init; }

    public required Guid PublicId { get; init; }

    public required int TemplateSectionId { get; init; }

    public required string Name { get; init; }

    public required SectionRole Role { get; init; }

    public required int MinInstances { get; init; }

    public int? MaxInstances { get; init; }

    public required int InitialInstances { get; init; }

    public required int DisplayOrder { get; init; }

    public SheetLock? Lock { get; init; }

    public required bool IsPending { get; init; }

    public required bool CanRemove { get; init; }

    public required IReadOnlyList<SheetRow> Rows { get; init; }

    public required IReadOnlyList<SheetSection> Sections { get; init; }

    public required IReadOnlyList<AddableSection> AddableSections { get; init; }

    public SheetChange? LastChange { get; init; }
}
