using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace PUSpecSheet.Api.Common.Logging;

public sealed class IdentityLogEnrichmentFilter : ILogEventEnricher
{
    private const string NAME_CLAIM_TYPE = "name";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityLogEnrichmentFilter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var username = TryGetName(_httpContextAccessor);
        username ??= "Unknown";

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("identity", username));
    }

    private static string? TryGetName(IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor?.HttpContext?.User.Identity is not ClaimsIdentity claimsIdentity)
        {
            return null;
        }

        var nameClaim = claimsIdentity.Claims.FirstOrDefault(claim => string.Equals(claim.Type, NAME_CLAIM_TYPE, StringComparison.OrdinalIgnoreCase));
        return nameClaim?.Value;
    }
}
