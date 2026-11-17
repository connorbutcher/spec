using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Domain.Properties.Table;

[JsonDerivedType(typeof(LinkedTableProperties), typeDiscriminator: "linked")]
public interface ITableProperties
{
}
