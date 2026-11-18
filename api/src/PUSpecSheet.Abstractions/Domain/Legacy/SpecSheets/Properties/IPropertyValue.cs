using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets.Properties;

[JsonDerivedType(typeof(StringPropertyValue), "string")]
[JsonDerivedType(typeof(IntPropertyValue), "int")]
[JsonDerivedType(typeof(DoublePropertyValue), "double")]
[JsonDerivedType(typeof(BoolPropertyValue), "bool")]
[JsonDerivedType(typeof(DateTimePropertyValue), "datetime")]
public interface IPropertyValue
{
}
