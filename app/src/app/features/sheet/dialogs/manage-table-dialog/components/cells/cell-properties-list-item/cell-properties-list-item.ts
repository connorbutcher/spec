import { Component, computed, input, output } from '@angular/core';
import { ConfigurableCellGroup } from '../../../../../models/configuration/configurable-cell-group';
import { ConfigurationGroup } from '../../../../../models/configuration/configuration-group';

@Component({
  selector: 'app-cell-properties-list-item',
  templateUrl: './cell-properties-list-item.html',
  styleUrl: './cell-properties-list-item.scss',
})
export class CellPropertiesListItem {
  public readonly cell =
    input.required<ConfigurationGroup<ConfigurableCellGroup>>();
  public readonly selectedId = input<string | undefined>();
  public readonly selectedGroup =
    output<ConfigurationGroup<ConfigurableCellGroup>>();
  public readonly requiresConfiguration = computed(() =>
    this.cell().value.cells.some((c) => !c.properties()?.isValid()),
  );

  public selectCell(): void {
    this.selectedGroup.emit(this.cell());
  }
}
