import { SpecSheetPropertyValue } from '../models/domain/spec-sheet/properties/spec-sheet-property-value';
import { SpecSheetPropertyValueSummary } from '../models/domain/spec-sheet/properties/spec-sheet-property-value-summary';
import { SpecSheet } from '../models/domain/spec-sheet/spec-sheet';
import { SpecSheetSummary } from '../models/domain/spec-sheet/spec-sheet-summary';
import { SpecSheetType } from '../models/domain/types/spec-sheet-type';
import { StructureLevelType } from '../types/configuration/structure-level-type.enum';

export class SpecSheetHelpers {
  public static getPropertiesForSpecSheetSummary(
    specSheet: SpecSheetSummary,
    specSheetType: SpecSheetType,
  ): SpecSheetPropertyValueSummary[] {
    const displayLevel = specSheetType.structureLevels.find(
      (sl) => sl.structureLevelType === StructureLevelType.Display,
    );
    if (!displayLevel) {
      return [];
    }

    const properties: SpecSheetPropertyValueSummary[] = [];
    for (const levelValue of displayLevel.levelValues) {
      const property = specSheet.propertyValues.find(
        (pv) => pv.referenceId === levelValue.specSheetTypePropertyReferenceId,
      );
      if (property) {
        properties.push(property);
      }
    }

    return properties;
  }

  public static getPropertiesForSpecSheet(
    specSheet: SpecSheet,
    specSheetType: SpecSheetType,
  ): SpecSheetPropertyValue[] {
    const displayLevel = specSheetType.structureLevels.find(
      (sl) => sl.structureLevelType === StructureLevelType.Display,
    );
    if (!displayLevel) {
      return [];
    }

    const properties: SpecSheetPropertyValue[] = [];
    for (const levelValue of displayLevel.levelValues) {
      const property = specSheet.propertyValues.find(
        (pv) =>
          pv.specSheetTypeProperty.referenceId ===
          levelValue.specSheetTypePropertyReferenceId,
      );
      if (property) {
        properties.push(property);
      }
    }

    return properties;
  }

  public static sortSummaries(
    specSheets: SpecSheetSummary[],
    specSheetType: SpecSheetType,
  ): SpecSheetSummary[] {
    const displayLevel = specSheetType.structureLevels.find(
      (sl) => sl.structureLevelType === StructureLevelType.Display,
    );
    if (!displayLevel) {
      return specSheets;
    }

    const labels: { label: string; specSheet: SpecSheetSummary }[] = [];
    for (const specSheet of specSheets) {
      let label = `${specSheet.id}`;
      for (const levelValue of displayLevel.levelValues) {
        const property = specSheet.propertyValues.find(
          (pv) =>
            pv.referenceId === levelValue.specSheetTypePropertyReferenceId,
        );
        const propertyValue = (property?.propertyValue as any)?.value ?? '';
        label += label ? ` - ${propertyValue}` : propertyValue;
      }

      labels.push({ label, specSheet });
    }

    return labels
      .sort((x, y) => {
        switch (displayLevel.sortOrientation) {
          case 0:
            return x.label.localeCompare(y.label, undefined, { numeric: true });
          case 1:
          default:
            return y.label.localeCompare(x.label, undefined, { numeric: true });
        }
      })
      .map((x) => x.specSheet);
  }
}
