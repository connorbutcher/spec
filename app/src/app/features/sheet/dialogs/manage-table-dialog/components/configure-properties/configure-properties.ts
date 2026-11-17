import {
  Component,
  effect,
  input,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { ConfigurationGroup } from '../../../../models/configuration/configuration-group';
import { ConfigureCellProperties } from '../cells/configure-cell-properties/configure-cell-properties';
import { ConfigureTableProperties } from '../tables/configure-table-properties/configure-table-properties';

@Component({
  selector: 'app-configure-properties',
  templateUrl: './configure-properties.html',
  styleUrl: './configure-properties.scss',
})
export class ConfigureProperties {
  public readonly group = input.required<ConfigurationGroup<any>>();
  public readonly expandByDefault = input<boolean>(false);

  public readonly propertyContent = viewChild.required('propertyContent', {
    read: ViewContainerRef,
  });

  constructor() {
    effect(() => {
      const group = this.group();
      const propertyContent = this.propertyContent();
      if (!group || !propertyContent) {
        return;
      }

      this.addPropertyComponent();
    });
  }

  private addPropertyComponent(): void {
    const propertyContent = this.propertyContent();
    if (!propertyContent) {
      return;
    }

    propertyContent.clear();

    const group = this.group();
    switch (group.type) {
      case 'cell': {
        const componentRef = propertyContent.createComponent(
          ConfigureCellProperties,
        );
        componentRef.setInput('cellGroup', group);
        componentRef.setInput('expandByDefault', this.expandByDefault());
        break;
      }
      case 'table': {
        const componentRef = propertyContent.createComponent(
          ConfigureTableProperties,
        );
        componentRef.setInput('tableGroup', group);
        componentRef.setInput('expandByDefault', this.expandByDefault());
        break;
      }
    }
  }
}
