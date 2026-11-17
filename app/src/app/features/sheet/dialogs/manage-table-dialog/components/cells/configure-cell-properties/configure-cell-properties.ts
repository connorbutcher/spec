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
import { CellPropertiesComponentHelper } from '../../../../../features/cell-properties/helpers/cell-properties-component-helper';
import { ConfigurableCellGroup } from '../../../../../models/configuration/configurable-cell-group';
import { ConfigurationGroup } from '../../../../../models/configuration/configuration-group';

@Component({
  selector: 'app-configure-cell-properties',
  imports: [Tag],
  templateUrl: './configure-cell-properties.html',
  styleUrl: './configure-cell-properties.scss',
})
export class ConfigureCellProperties {
  public readonly cellGroup =
    input.required<ConfigurationGroup<ConfigurableCellGroup>>();
  public readonly isValid = computed(() =>
    this.cellGroup().value.cells.every((c) => c.properties()?.isValid()),
  );
  public readonly expandByDefault = input<boolean>(false);

  public readonly isExpanded = linkedSignal(() => this.expandByDefault());
  public readonly propertyContent = viewChild.required('propertyContent', {
    read: ViewContainerRef,
  });

  constructor() {
    effect(() => {
      const cellGroup = this.cellGroup();
      const propertyContent = this.propertyContent();
      if (!cellGroup || !propertyContent) {
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

    const component = CellPropertiesComponentHelper.getCellTypeComponent(
      this.cellGroup().value.tableCellType.key,
    );
    if (!component) {
      return;
    }

    const componentRef = propertyContent.createComponent(component);
    componentRef.setInput(
      'cellProperty',
      this.cellGroup().value.cells[0].properties(),
    );
  }
}
