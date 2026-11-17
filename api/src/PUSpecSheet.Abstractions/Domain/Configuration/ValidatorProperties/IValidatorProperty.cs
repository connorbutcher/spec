using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Domain.Configuration.ValidatorProperties;

[JsonDerivedType(typeof(CellsRequiredValidatorProperty), "cellsRequired")]
[JsonDerivedType(typeof(CellValueOrderValidatorProperty), "cellValueOrder")]
[JsonDerivedType(typeof(MinMaxLengthValidatorProperty), "minMaxLength")]
[JsonDerivedType(typeof(MinMaxNumberValidatorProperty), "minMaxNumber")]
public interface IValidatorProperty
{
}
