using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates.Sections;

internal static class SectionInstanceRules
{
    public static void ApplyHeader(DbTemplateSection section)
    {
        section.Role = SectionRole.Header;
        section.MinInstances = 1;
        section.MaxInstances = 1;
        section.InitialInstances = 1;
    }

    public static void ApplyAddable(UpdateTemplateSectionRequest request, DbTemplateSection section)
    {
        EnsureValid(request.MinInstances, request.MaxInstances, request.InitialInstances);

        section.Role = SectionRole.Addable;
        section.MinInstances = request.MinInstances;
        section.MaxInstances = request.MaxInstances;
        section.InitialInstances = request.InitialInstances;
    }

    public static void ApplyNewAddable(DbTemplateSection section)
    {
        section.Role = SectionRole.Addable;
        section.MinInstances = 0;
        section.MaxInstances = null;
        section.InitialInstances = 0;
    }

    public static void EnsureValid(int minInstances, int? maxInstances, int initialInstances)
    {
        if (minInstances > initialInstances)
        {
            throw new InvalidRequestException("A table can't start with fewer copies than the fewest allowed.");
        }

        if (maxInstances is not null && initialInstances > maxInstances)
        {
            throw new InvalidRequestException("A table can't start with more copies than the most allowed.");
        }
    }
}
