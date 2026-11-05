using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Seeding;

/// <summary>One of the showcase's table templates: where it goes and how each of its versions is laid out.</summary>
public interface IShowcaseTemplate
{
    string SheetTypeName { get; }

    string Name { get; }

    TemplateOrientation Orientation { get; }

    /// <summary>How many of the table's own columns stay pinned while the rest scroll.</summary>
    int StickyColumnCount { get; }

    /// <summary>Lays out version <paramref name="versionNumber"/>, counting from 1.</summary>
    void Lay(TemplateLayout layout, int versionNumber);
}
