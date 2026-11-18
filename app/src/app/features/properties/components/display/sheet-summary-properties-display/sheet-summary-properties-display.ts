import {
  AfterViewInit,
  Component,
  computed,
  effect,
  input,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { SpecSheetHelpers } from '../../../../../core/helpers/spec-sheet-helpers';
import { SpecSheetType } from '../../../../../core/models/domain/types/spec-sheet-type';
import { PropertyControlTypeHelpers } from '../../../helpers/property-control-type-helpers';
import { SheetListItem } from '../../../../../core/models/domain/sheets/sheet-list-item';

@Component({
  selector: 'app-sheet-summary-properties-display',
  templateUrl: './sheet-summary-properties-display.html',
  styleUrl: './sheet-summary-properties-display.scss',
})
export class SheetSummaryPropertiesDisplay implements AfterViewInit {
  public readonly container = viewChild.required('container', {
    read: ViewContainerRef,
  });
  public readonly specSheet = input.required<SheetListItem>();
  public readonly specSheetType = input.required<SpecSheetType>();

  private readonly _properties = computed(() =>
    SpecSheetHelpers.getPropertiesForSpecSheetSummary(
      this.specSheet(),
      this.specSheetType(),
    ),
  );

  constructor() {
    effect(() => {
      this._properties();
      this.loadPropertyControls();
    });
  }

  ngAfterViewInit(): void {
    this.loadPropertyControls();
  }

  private loadPropertyControls(): void {
    const properties = this._properties();
    const container = this.container();
    if (!container || !properties || properties.length === 0) {
      return;
    }

    container.clear();

    for (const property of properties) {
      const component = PropertyControlTypeHelpers.getViewComponent(
        property.valueControlType,
      );
      if (component) {
        const componentRef = container.createComponent(component);
        componentRef.setInput('property', property);
      }
    }
  }
}
