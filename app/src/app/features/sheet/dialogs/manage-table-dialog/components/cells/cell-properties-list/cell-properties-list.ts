import { Component, input, output } from '@angular/core';
import { ConfigurableCellGroup } from '../../../../../models/configuration/configurable-cell-group';
import { ConfigurationGroup } from '../../../../../models/configuration/configuration-group';
import { CellPropertiesListItem } from '../cell-properties-list-item/cell-properties-list-item';

@Component({
  selector: 'app-cell-properties-list',
  imports: [CellPropertiesListItem],
  templateUrl: './cell-properties-list.html',
  styleUrl: './cell-properties-list.scss',
})
export class CellPropertiesList {
  public readonly cells =
    input.required<ConfigurationGroup<ConfigurableCellGroup>[]>();
  public readonly selectedId = input<string | undefined>();
  public readonly selectedGroup =
    output<ConfigurationGroup<ConfigurableCellGroup>>();

  public selectGroup(group: ConfigurationGroup<ConfigurableCellGroup>): void {
    this.selectedGroup.emit(group);
  }
}
