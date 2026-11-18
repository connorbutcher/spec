import { SheetProperty } from '../models/domain/sheets/sheet';
import { SpecSheetTypeProperty } from '../models/domain/types/spec-sheet-type-property';
import { PropertyValueType } from '../types/properties/property-value-type.enum';

/**
 * A sheet property stores its value as text; the value type of the type property says how to read it.
 */
export class SheetPropertyHelpers {
  public static getValue<T = string | number | boolean>(
    property: SheetProperty | undefined,
    typeProperty?: SpecSheetTypeProperty,
  ): T | undefined {
    const raw = property?.value;
    if (raw === undefined || raw === null) {
      return undefined;
    }

    const valueType = (typeProperty ?? property?.specSheetTypeProperty)
      ?.valueType;
    switch (valueType) {
      case PropertyValueType.Int:
        return Number.parseInt(raw, 10) as T;
      case PropertyValueType.Double:
        return Number.parseFloat(raw) as T;
      case PropertyValueType.Boolean:
        return (raw.toLowerCase() === 'true') as T;
      default:
        return raw as T;
    }
  }

  /** The property's value as display text, or a fallback when it has none. */
  public static getText(
    property: SheetProperty | undefined,
    fallback = '',
    typeProperty?: SpecSheetTypeProperty,
  ): string {
    const value = SheetPropertyHelpers.getValue(property, typeProperty);
    return value === undefined ? fallback : String(value);
  }

  /** The property of the sheet whose type property has the reference id. */
  public static findByReferenceId(
    properties: SheetProperty[],
    typeProperties: SpecSheetTypeProperty[],
    referenceId: string,
  ): SheetProperty | undefined {
    const typeProperty = typeProperties.find(
      (tp) => tp.referenceId === referenceId,
    );
    return properties.find(
      (p) =>
        p.specSheetTypePropertyId === typeProperty?.id ||
        p.specSheetTypeProperty?.referenceId === referenceId,
    );
  }

  /** The property with its type property set, for display when the list it came from left it out. */
  public static withTypeProperty(
    property: SheetProperty,
    typeProperties: SpecSheetTypeProperty[],
  ): SheetProperty {
    if (property.specSheetTypeProperty) {
      return property;
    }

    return {
      ...property,
      specSheetTypeProperty: typeProperties.find(
        (tp) => tp.id === property.specSheetTypePropertyId,
      ),
    };
  }
}
