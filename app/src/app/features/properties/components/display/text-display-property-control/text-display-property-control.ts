import { Component, computed, input } from '@angular/core';
import { Tooltip } from 'primeng/tooltip';
import { SpecSheetPropertyValueSummary } from '../../../../../core/models/domain/spec-sheet/properties/spec-sheet-property-value-summary';
import { StringPropertyValue } from '../../../../../core/models/domain/spec-sheet/properties/values/string-property-value';

@Component({
  selector: 'app-text-display-property-control',
  templateUrl: './text-display-property-control.html',
  styleUrl: './text-display-property-control.scss',
  imports: [Tooltip],
})
export class TextDisplayPropertyControl {
  public readonly property = input.required<SpecSheetPropertyValueSummary>();
  public readonly displayValue = computed(() => {
    const property = this.property();
    if (!property) {
      return 'N/A';
    }

    const propertyValue = property.propertyValue as StringPropertyValue;
    return propertyValue.value;
  });
  public readonly classes = input<string>();
}
