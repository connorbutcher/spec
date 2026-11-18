import { SpecSheetType } from '../models/domain/types/spec-sheet-type';
import { SheetPropertyHelpers } from './sheet-property-helpers';
import { StructureLevelType } from '../types/configuration/structure-level-type.enum';
import { Sheet } from '../models/domain/sheets/sheet';
import { SheetListItem } from '../models/domain/sheets/sheet-list-item';
import { SheetProperty } from '../models/domain/sheets/sheet-property';

export class SpecSheetHelpers {
  public static getPropertiesForSpecSheetSummary(
    specSheet: SheetListItem,
    specSheetType: SpecSheetType,
  ): SheetProperty[] {
    const displayLevel = specSheetType.structureLevels.find(
      (sl) => sl.structureLevelType === StructureLevelType.Display,
    );
    if (!displayLevel) {
      return [];
    }

    const properties: SheetProperty[] = [];
    for (const levelValue of displayLevel.levelValues) {
      const property = SheetPropertyHelpers.findByReferenceId(
        specSheet.properties,
        specSheetType.properties,
        levelValue.specSheetTypePropertyReferenceId,
      );
      if (property) {
        properties.push(
          SheetPropertyHelpers.withTypeProperty(
            property,
            specSheetType.properties,
          ),
        );
      }
    }

    return properties;
  }

  public static getPropertiesForSpecSheet(
    specSheet: Sheet,
    specSheetType: SpecSheetType,
  ): SheetProperty[] {
    const displayLevel = specSheetType.structureLevels.find(
      (sl) => sl.structureLevelType === StructureLevelType.Display,
    );
    if (!displayLevel) {
      return [];
    }

    const properties: SheetProperty[] = [];
    for (const levelValue of displayLevel.levelValues) {
      const property = SheetPropertyHelpers.findByReferenceId(
        specSheet.properties,
        specSheetType.properties,
        levelValue.specSheetTypePropertyReferenceId,
      );
      if (property) {
        properties.push(
          SheetPropertyHelpers.withTypeProperty(
            property,
            specSheetType.properties,
          ),
        );
      }
    }

    return properties;
  }

  public static sortSummaries(
    specSheets: SheetListItem[],
    specSheetType: SpecSheetType,
  ): SheetListItem[] {
    const displayLevel = specSheetType.structureLevels.find(
      (sl) => sl.structureLevelType === StructureLevelType.Display,
    );
    if (!displayLevel) {
      return specSheets;
    }

    const labels: { label: string; specSheet: SheetListItem }[] = [];
    for (const specSheet of specSheets) {
      let label = `${specSheet.versionNumber}`;
      for (const levelValue of displayLevel.levelValues) {
        const property = SheetPropertyHelpers.findByReferenceId(
          specSheet.properties,
          specSheetType.properties,
          levelValue.specSheetTypePropertyReferenceId,
        );
        const propertyValue = SheetPropertyHelpers.getText(property);
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
