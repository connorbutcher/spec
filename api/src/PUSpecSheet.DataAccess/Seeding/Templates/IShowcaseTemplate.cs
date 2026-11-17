using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Seeding.Templates;

public interface IShowcaseTemplate
{
    string SheetTypeName { get; }

    string Name { get; }

    TemplateOrientation Orientation { get; }

    int StickyColumnCount { get; }

    void Lay(TemplateLayout layout, int versionNumber);
}
