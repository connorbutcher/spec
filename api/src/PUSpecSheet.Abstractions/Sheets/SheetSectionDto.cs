using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>
/// A section copy on a sheet table. Its own rows come first, then its sub-sections. The template's
/// name, role and instance counts are carried along so the screen can lay it out like the template.
/// </summary>
public sealed record SheetSectionDto
{
    public SheetSectionDto(
        int id,
        Guid publicId,
        int templateSectionId,
        string name,
        SectionRole role,
        int minInstances,
        int? maxInstances,
        int initialInstances,
        int displayOrder,
        SheetLockDto? @lock,
        bool isPending,
        bool canRemove,
        IReadOnlyList<SheetRowDto> rows,
        IReadOnlyList<SheetSectionDto> sections,
        IReadOnlyList<AddableSectionDto> addableSections,
        SheetChangeDto? lastChange)
    {
        Id = id;
        PublicId = publicId;
        TemplateSectionId = templateSectionId;
        Name = name;
        Role = role;
        MinInstances = minInstances;
        MaxInstances = maxInstances;
        InitialInstances = initialInstances;
        DisplayOrder = displayOrder;
        Lock = @lock;
        IsPending = isPending;
        CanRemove = canRemove;
        Rows = rows;
        Sections = sections;
        AddableSections = addableSections;
        LastChange = lastChange;
    }

    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int TemplateSectionId { get; init; }

    public string Name { get; init; }

    public SectionRole Role { get; init; }

    public int MinInstances { get; init; }

    public int? MaxInstances { get; init; }

    public int InitialInstances { get; init; }

    public int DisplayOrder { get; init; }

    public SheetLockDto? Lock { get; init; }

    public bool IsPending { get; init; }

    public bool CanRemove { get; init; }

    public IReadOnlyList<SheetRowDto> Rows { get; init; }

    public IReadOnlyList<SheetSectionDto> Sections { get; init; }

    public IReadOnlyList<AddableSectionDto> AddableSections { get; init; }

    public SheetChangeDto? LastChange { get; init; }
}
