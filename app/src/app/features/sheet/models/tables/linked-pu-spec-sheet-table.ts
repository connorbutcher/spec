import { computed, linkedSignal } from '@angular/core';
import { ValidatorLevel } from '../../../../core/types/configuration/validator-level.enum';
import { LinkedTableProperty } from '../../features/table-properties/models/linked-table-property';
import { SpecSheetAdvisory } from '../spec-sheet-advisory';
import { PUSpecSheetTableBase } from './pu-spec-sheet-table-base';

export class LinkedPUSpecSheetTable extends PUSpecSheetTableBase<LinkedTableProperty> {
  public override readonly headerColorClass: string =
    'var(--theme-table-border-color-purple)';
  public override readonly canUserEdit = false;
  public override readonly requiresUserConfiguration = computed(
    () => this.tableType().requiresUserConfiguration,
  );
  public override readonly configurableCells = computed(() => []);
  public override readonly cellsNeedingConfiguration = computed(() => 0);
  public override readonly isValid = computed(
    () => this.advisories().length === 0,
  );
  public override readonly hasChanges = computed(() => {
    if (!this.table) {
      return true;
    }

    const propertyChanges = this.properties()?.hasChanges() ?? false;

    return (
      this.referenceId() !== this.table.referenceId ||
      this.title() !== this.table.title ||
      this.displayOrder() !== this.table.displayOrder ||
      propertyChanges
    );
  });
  public override readonly title = linkedSignal(() => {
    const properties = this.properties();
    if (!properties) {
      return '';
    }

    return properties.table()?.title() || '';
  });
  public override readonly groups = linkedSignal(
    () => this.properties()?.table()?.groups() || [],
  );
  public override readonly widgets = linkedSignal(
    () => this.properties()?.table()?.widgets() || [],
  );
  public override readonly columnDefinitions = computed(() => {
    const properties = this.properties();
    if (!properties || !properties.isValid()) {
      return { count: 1, definitionString: '1fr' };
    }

    return (
      properties.table()?.columnDefinitions() || {
        count: 1,
        definitionString: '1fr',
      }
    );
  });
  public override readonly advisories = computed<SpecSheetAdvisory[]>(() => {
    const advisories: SpecSheetAdvisory[] = [];

    if (this.properties() && !this.properties()?.isValid()) {
      advisories.push({
        level: ValidatorLevel.Error,
        message: 'Table properties are not valid.',
        referenceId: this.referenceId(),
      });
    }

    for (const widget of this.widgets()) {
      advisories.push(...widget.advisories());
    }

    if (this.title().trim().length === 0) {
      advisories.push({
        level: ValidatorLevel.Error,
        message: 'Title is required.',
        referenceId: this.referenceId(),
      });
    }

    if (this.groups().length === 0) {
      advisories.push({
        level: ValidatorLevel.Error,
        message: 'At least one row is required.',
        referenceId: this.referenceId(),
      });
    }

    return advisories;
  });
}
