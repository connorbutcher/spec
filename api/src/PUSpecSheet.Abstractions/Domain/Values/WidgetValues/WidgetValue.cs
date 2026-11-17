using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

[JsonDerivedType(typeof(PartPairWidgetValue), typeDiscriminator: "partPair")]
[JsonDerivedType(typeof(PFKNominalWidgetValue), typeDiscriminator: "pfkNominal")]
public record WidgetValue
{
}
