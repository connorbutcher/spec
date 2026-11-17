import { Component, computed, input, output } from '@angular/core';
import { ConfigurableTableGroup } from '../../../../../models/configuration/configurable-table-group';
import { ConfigurationGroup } from '../../../../../models/configuration/configuration-group';

@Component({
  selector: 'app-table-properties-list-item',
  templateUrl: './table-properties-list-item.html',
  styleUrl: './table-properties-list-item.scss',
})
export class TablePropertiesListItem {
  public readonly table =
    input.required<ConfigurationGroup<ConfigurableTableGroup>>();
  public readonly selectedId = input<string | undefined>();
  public readonly selectedGroup =
    output<ConfigurationGroup<ConfigurableTableGroup>>();
  public readonly requiresConfiguration = computed(
    () => !this.table().value.table.properties()?.isValid(),
  );

  public selectTable(): void {
    this.selectedGroup.emit(this.table());
  }
}
