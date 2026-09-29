using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

/// <summary>Checks and applies a section's role and how many copies of it a sheet table may hold.</summary>
internal static class SectionInstanceRules
{
    /// <summary>
    /// A top-level fixed section is the table's header, so it's always exactly one copy. Any other fixed
    /// section is at most one copy; a repeating one takes the counts as given.
    /// </summary>
    public static void Apply(UpdateTemplateSectionRequest request, TemplateSection section, bool isTopLevel)
    {
        var isHeader = isTopLevel && request.Role == SectionRole.Fixed;
        var min = isHeader ? 1 : request.MinInstances;
        var initial = isHeader ? 1 : request.InitialInstances;
        var max = request.Role == SectionRole.Fixed ? 1 : request.MaxInstances;

        if (min > initial)
        {
            throw new InvalidRequestException("A table can't start with fewer copies than the minimum.");
        }

        if (max is not null && initial > max)
        {
            throw new InvalidRequestException(request.Role == SectionRole.Fixed
                ? "A fixed section is a single block, so a table starts with at most one."
                : "A table can't start with more copies than the maximum.");
        }

        section.Role = request.Role;
        section.MinInstances = min;
        section.MaxInstances = max;
        section.InitialInstances = initial;
    }
}
