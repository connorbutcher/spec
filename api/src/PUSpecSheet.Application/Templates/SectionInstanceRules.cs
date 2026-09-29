using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

/// <summary>Checks and applies a section's role and how many copies of it a sheet table may hold.</summary>
internal static class SectionInstanceRules
{
    public static void Apply(UpdateTemplateSectionRequest request, TemplateSection section)
    {
        // A fixed section is a single block: at most one copy, so its maximum is always 1.
        var max = request.Role == SectionRole.Fixed ? 1 : request.MaxInstances;

        if (request.MinInstances > request.InitialInstances)
        {
            throw new InvalidRequestException("A table can't start with fewer copies than the minimum.");
        }

        if (max is not null && request.InitialInstances > max)
        {
            throw new InvalidRequestException(request.Role == SectionRole.Fixed
                ? "A fixed section is a single block, so a table starts with at most one."
                : "A table can't start with more copies than the maximum.");
        }

        section.Role = request.Role;
        section.MinInstances = request.MinInstances;
        section.MaxInstances = max;
        section.InitialInstances = request.InitialInstances;
    }
}
