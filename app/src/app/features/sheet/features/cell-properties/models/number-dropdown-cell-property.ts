import { computed, linkedSignal } from '@angular/core';
import { CellProperties } from '../../../../../core/models/domain/properties/cell/cell-properties';
import { NumberDropdownCellProperties } from '../../../../../core/models/domain/properties/cell/number-dropdown-cell-properties';
import { TableCellTemplate } from '../../../../../core/models/domain/template/table-cell-template';
import { CellPropertyBase } from './cell-property-base';

export class NumberDropdownCellProperty extends CellPropertyBase<NumberDropdownCellProperties> {
  public override readonly isValid = computed(
    () => !!this.options() && this.options().length > 0,
  );
  public override readonly hasChanges = computed(
    () =>
      this.defaultValue() !== this.properties.defaultValue ||
      JSON.stringify(this.options()) !==
        JSON.stringify(this.properties.options),
  );
  public override readonly internalReferences = computed(() => []);

  public readonly options = linkedSignal<number[]>(() => {
    return this.properties.options;
  });
  public readonly defaultValue = linkedSignal<number | undefined>(() => {
    return this.properties.defaultValue;
  });

  public override set(value: NumberDropdownCellProperties): void {
    this.options.set(value.options);
    this.defaultValue.set(value.defaultValue);
  }

  public override toCellTemplate(): TableCellTemplate {
    return {
      ...this.cellTemplate!,
      cellConfiguration: {
        ...this.cellTemplate!.cellConfiguration!,
        properties: this.toCellConfiguration(),
      },
    };
  }

  public override toCellConfiguration(): CellProperties {
    return {
      ...this.properties,
      options: this.options(),
      defaultValue: this.defaultValue(),
    } as NumberDropdownCellProperties;
  }
}
