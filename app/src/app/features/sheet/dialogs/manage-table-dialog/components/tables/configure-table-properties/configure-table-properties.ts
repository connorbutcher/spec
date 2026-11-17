import {
  Component,
  computed,
  effect,
  input,
  linkedSignal,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { Tag } from 'primeng/tag';
import { TablePropertiesComponentHelper } from '../../../../../features/table-properties/helpers/table-properties-component-helper';
import { ConfigurableTableGroup } from '../../../../../models/configuration/configurable-table-group';
import { ConfigurationGroup } from '../../../../../models/configuration/configuration-group';

@Component({
  selector: 'app-configure-table-properties',
  imports: [Tag],
  templateUrl: './configure-table-properties.html',
  styleUrl: './configure-table-properties.scss',
})
export class ConfigureTableProperties {
  public readonly tableGroup =
    input.required<ConfigurationGroup<ConfigurableTableGroup>>();
  public readonly isValid = computed(() =>
    this.tableGroup().value.table.properties()?.isValid(),
  );
  public readonly expandByDefault = input<boolean>(false);

  public readonly isExpanded = linkedSignal(() => this.expandByDefault());
  public readonly propertyContent = viewChild.required('propertyContent', {
    read: ViewContainerRef,
  });

  constructor() {
    effect(() => {
      const tableGroup = this.tableGroup();
      const propertyContent = this.propertyContent();
      if (!tableGroup || !propertyContent) {
        return;
      }

      this.addPropertyComponent();
    });
  }

  public toggleExpansion(): void {
    this.isExpanded.set(!this.isExpanded());

    if (this.isExpanded()) {
      this.addPropertyComponent();
    } else {
      this.propertyContent().clear();
    }
  }

  private addPropertyComponent(): void {
    const propertyContent = this.propertyContent();
    if (!propertyContent) {
      return;
    }

    propertyContent.clear();

    const component = TablePropertiesComponentHelper.getTableTypeComponent(
      this.tableGroup().value.table.tableType(),
    );
    if (!component) {
      return;
    }

    const componentRef = propertyContent.createComponent(component);
    componentRef.setInput(
      'tableProperty',
      this.tableGroup().value.table.properties(),
    );
  }
}
