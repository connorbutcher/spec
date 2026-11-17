import { SpecSheetPropertyValue } from '../../../core/models/domain/spec-sheet/properties/spec-sheet-property-value';
import { SpecSheetPropertyValueSummary } from '../../../core/models/domain/spec-sheet/properties/spec-sheet-property-value-summary';

export class PropertyValueHelpers {
  public static getValue<T>(
    property: SpecSheetPropertyValue | SpecSheetPropertyValueSummary,
  ): T | undefined {
    if (!property || !property.propertyValue) {
      return undefined;
    }

    switch (property.propertyValue.$type) {
      case 'string':
      case 'int':
      case 'bool':
      case 'datetime':
      case 'double':
        return (property.propertyValue as any).value as T;
      default:
        return undefined;
    }
  }
}
