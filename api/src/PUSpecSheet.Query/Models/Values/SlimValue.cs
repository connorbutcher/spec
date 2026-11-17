using System.Text.Json.Serialization;

namespace PUSpecSheet.Query.Models.Values;

[JsonDerivedType(typeof(SlimTextValue), typeDiscriminator: "text")]
[JsonDerivedType(typeof(SlimDoubleValue), typeDiscriminator: "double")]
[JsonDerivedType(typeof(SlimIntValue), typeDiscriminator: "int")]
[JsonDerivedType(typeof(SlimBoolValue), typeDiscriminator: "bool")]
[JsonDerivedType(typeof(SlimDateTimeValue), typeDiscriminator: "datetime")]
[JsonDerivedType(typeof(SlimPairValue), typeDiscriminator: "pair")]
[JsonDerivedType(typeof(SlimLinkedValue), typeDiscriminator: "linked")]
public record SlimValue
{
    /// <summary>
    /// Gets the unique identifier of the simple value.
    /// </summary>
    public required int Id { get; init; }
}
