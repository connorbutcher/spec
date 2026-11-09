using Microsoft.Extensions.DependencyInjection;

namespace PUSpecSheet.Application.Approvals;

public static class ApprovalServiceCollectionExtensions
{
    /// <summary>Registers publishing with approval: submitting, deciding and listing submissions.</summary>
    public static IServiceCollection AddPuSpecSheetApprovals(this IServiceCollection services)
    {
        services.AddScoped<ApproverDirectory>();
        services.AddScoped<SheetSubmitter>();
        services.AddScoped<SubmissionDecider>();
        services.AddScoped<SubmissionQueries>();
        services.AddScoped<ISheetSubmissionService, SheetSubmissionService>();

        return services;
    }
}
