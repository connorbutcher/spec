using Serilog.Core;
using Serilog.Events;

namespace PUSpecSheet.Api.Common.Logging;

internal class HostLogEnrichmentFilter : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var host = Environment.MachineName;
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("hostname", host));
    }
}
