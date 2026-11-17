import {
  AfterViewInit,
  Component,
  input,
  linkedSignal,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { Tag } from 'primeng/tag';
import { TableProperties } from '../../../../../../core/models/domain/properties/table/table-properties';
import { TablePropertiesComponentHelper } from '../../helpers/table-properties-component-helper';
import { TablePropertyBase } from '../../models/table-property-base';

@Component({
  selector: 'app-manage-table-properties',
  imports: [Tag],
  templateUrl: './manage-table-properties.html',
  styleUrl: './manage-table-properties.scss',
})
export class ManageTableProperties implements AfterViewInit {
  public readonly tableProperty =
    input.required<TablePropertyBase<TableProperties>>();
  public readonly expandByDefault = input<boolean>(false);

  public readonly isExpanded = linkedSignal(() => this.expandByDefault());
  public readonly propertyContent = viewChild.required('propertyContent', {
    read: ViewContainerRef,
  });

  ngAfterViewInit(): void {
    if (this.isExpanded()) {
      this.addPropertyComponent();
    }
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
      this.tableProperty().tableTemplate,
    );
    if (!component) {
      return;
    }

    const componentRef = propertyContent.createComponent(component);
    componentRef.setInput('tableProperty', this.tableProperty());
  }
}
