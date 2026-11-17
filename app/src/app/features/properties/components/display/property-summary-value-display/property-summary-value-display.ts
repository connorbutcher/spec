import {
  AfterViewInit,
  Component,
  effect,
  input,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { SpecSheetPropertyValueSummary } from '../../../../../core/models/domain/spec-sheet/properties/spec-sheet-property-value-summary';
import { PropertyControlType } from '../../../../../core/types/properties/property-control-type.enum';
import { PropertyControlTypeHelpers } from '../../../helpers/property-control-type-helpers';

@Component({
  selector: 'app-property-summary-value-display',
  templateUrl: './property-summary-value-display.html',
  styleUrl: './property-summary-value-display.scss',
})
export class PropertySummaryValueDisplay implements AfterViewInit {
  public readonly container = viewChild.required('container', {
    read: ViewContainerRef,
  });
  public readonly propertyValue =
    input.required<SpecSheetPropertyValueSummary>();

  constructor() {
    effect(() => {
      this.propertyValue();
      this.loadPropertyControl();
    });
  }

  ngAfterViewInit(): void {
    this.loadPropertyControl();
  }

  private loadPropertyControl(): void {
    const propertyValue = this.propertyValue();
    const container = this.container();
    if (!container || !propertyValue) {
      return;
    }

    container.clear();

    const component = PropertyControlTypeHelpers.getViewComponent(
      propertyValue.valueControlType as PropertyControlType,
    );
    if (component) {
      const componentRef = container.createComponent(component);
      componentRef.setInput('property', propertyValue);
    }
  }
}
