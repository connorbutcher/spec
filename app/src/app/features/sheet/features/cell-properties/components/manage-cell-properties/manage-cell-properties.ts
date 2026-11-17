import {
  AfterViewInit,
  Component,
  input,
  linkedSignal,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { Tag } from 'primeng/tag';
import { CellProperties } from '../../../../../../core/models/domain/properties/cell/cell-properties';
import { CellPropertiesComponentHelper } from '../../helpers/cell-properties-component-helper';
import { CellPropertyBase } from '../../models/cell-property-base';

@Component({
  selector: 'app-manage-cell-properties',
  imports: [Tag],
  templateUrl: './manage-cell-properties.html',
  styleUrl: './manage-cell-properties.scss',
})
export class ManageCellProperties implements AfterViewInit {
  public readonly cellProperty =
    input.required<CellPropertyBase<CellProperties>>();
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

    const component = CellPropertiesComponentHelper.getCellTypeComponent(
      this.cellProperty().tableCellType.key,
    );
    if (!component) {
      return;
    }

    const componentRef = propertyContent.createComponent(component);
    componentRef.setInput('cellProperty', this.cellProperty());
  }
}
