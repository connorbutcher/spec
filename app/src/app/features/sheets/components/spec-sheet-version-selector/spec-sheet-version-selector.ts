import { Component, computed, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Select } from 'primeng/select';
import { SpecSheetSummary } from '../../../../core/models/domain/spec-sheet/spec-sheet-summary';
import { SpecSheetType } from '../../../../core/models/domain/types/spec-sheet-type';
import { StructureLevelType } from '../../../../core/types/configuration/structure-level-type.enum';
import { SheetsService } from '../../services/sheets.service';
import { SpecSheetVersion } from './spec-sheet-version/spec-sheet-version';

@Component({
  selector: 'app-spec-sheet-version-selector',
  imports: [FormsModule, Select, SpecSheetVersion],
  templateUrl: './spec-sheet-version-selector.html',
  styleUrl: './spec-sheet-version-selector.scss',
})
export class SpecSheetVersionSelector {
  public readonly specSheetType = input.required<SpecSheetType>();
  public readonly groupLevel = computed(() =>
    this.specSheetType().structureLevels.find(
      (level) =>
        level.structureLevelType === StructureLevelType.Group &&
        level.levelValues.length > 0,
    ),
  );
  public readonly options = computed(() => {
    const versions = this.specSheets();
    const groupLevel = this.groupLevel();
    if (!groupLevel) {
      return versions;
    }

    const propertyReferenceId =
      groupLevel.levelValues[0].specSheetTypePropertyReferenceId;
    const groups = new Map<
      string,
      { label: string; items: SpecSheetSummary[] }
    >();

    for (const version of versions) {
      const property = version.propertyValues.find(
        (value) => value.referenceId === propertyReferenceId,
      );
      const propertyValue = property?.propertyValue as
        | { value?: string | number | boolean | null }
        | undefined;
      const label = String(propertyValue?.value ?? 'N/A');
      const group = groups.get(label);

      if (group) {
        group.items.push(version);
      } else {
        groups.set(label, { label, items: [version] });
      }
    }

    return Array.from(groups.values()).sort((first, second) =>
      groupLevel.sortOrientation === 0
        ? first.label.localeCompare(second.label, undefined, { numeric: true })
        : second.label.localeCompare(first.label, undefined, { numeric: true }),
    );
  });

  private readonly _sheetsService = inject(SheetsService);

  public selectVersion(specSheet: SpecSheetSummary | null): void {
    this._sheetsService.selectVersion(specSheet?.id ?? null);
  }

  public get specSheets() {
    return this._sheetsService.specSheetsForPhaseAndType;
  }

  public get selectedSpecSheet() {
    return this._sheetsService.selectedSpecSheet;
  }
}
