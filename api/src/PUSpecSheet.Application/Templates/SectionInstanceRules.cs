using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

/// <summary>Checks and applies how many copies of a section a sheet table may hold.</summary>
internal static class SectionInstanceRules
{
    /// <summary>The header is always exactly one copy; its counts can't change.</summary>
    public static void ApplyHeader(TemplateSection section)
    {
        section.Role = SectionRole.Header;
        section.MinInstances = 1;
        section.MaxInstances = 1;
        section.InitialInstances = 1;
    }

    /// <summary>Applies an addable section's counts, which must satisfy min &lt;= starts with &lt;= max.</summary>
    public static void ApplyAddable(UpdateTemplateSectionRequest request, TemplateSection section)
    {
        if (request.MinInstances > request.InitialInstances)
        {
            throw new InvalidRequestException("A table can't start with fewer copies than the fewest allowed.");
        }

        if (request.MaxInstances is not null && request.InitialInstances > request.MaxInstances)
        {
            throw new InvalidRequestException("A table can't start with more copies than the most allowed.");
        }

        section.Role = SectionRole.Addable;
        section.MinInstances = request.MinInstances;
        section.MaxInstances = request.MaxInstances;
        section.InitialInstances = request.InitialInstances;
    }

    /// <summary>The counts a new addable section starts with: none until someone adds one on a sheet.</summary>
    public static void ApplyNewAddable(TemplateSection section)
    {
        section.Role = SectionRole.Addable;
        section.MinInstances = 0;
        section.MaxInstances = null;
        section.InitialInstances = 0;
    }
}
