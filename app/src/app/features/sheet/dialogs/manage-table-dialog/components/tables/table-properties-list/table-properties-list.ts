import { Component, input, output } from '@angular/core';
import { ConfigurableTableGroup } from '../../../../../models/configuration/configurable-table-group';
import { ConfigurationGroup } from '../../../../../models/configuration/configuration-group';
import { TablePropertiesListItem } from '../table-properties-list-item/table-properties-list-item';

@Component({
  selector: 'app-table-properties-list',
  imports: [TablePropertiesListItem],
  templateUrl: './table-properties-list.html',
  styleUrl: './table-properties-list.scss',
})
export class TablePropertiesList {
  public readonly tables =
    input.required<ConfigurationGroup<ConfigurableTableGroup>[]>();
  public readonly selectedId = input<string | undefined>();
  public readonly selectedGroup =
    output<ConfigurationGroup<ConfigurableTableGroup>>();

  public selectGroup(group: ConfigurationGroup<ConfigurableTableGroup>): void {
    this.selectedGroup.emit(group);
  }
}
