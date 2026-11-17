import { Component, computed, input } from '@angular/core';
import { Tooltip } from 'primeng/tooltip';
import { SpecSheetPropertyValueSummary } from '../../../../../core/models/domain/spec-sheet/properties/spec-sheet-property-value-summary';
import { IntPropertyValue } from '../../../../../core/models/domain/spec-sheet/properties/values/int-property-value';

@Component({
  selector: 'app-number-display-property-control',
  templateUrl: './number-display-property-control.html',
  styleUrl: './number-display-property-control.scss',
  imports: [Tooltip],
})
export class NumberDisplayPropertyControl {
  public readonly property = input.required<SpecSheetPropertyValueSummary>();
  public readonly displayValue = computed(() => {
    const instancePropertyValue = this.property();
    if (!instancePropertyValue) {
      return 'N/A';
    }

    const propertyValue =
      instancePropertyValue.propertyValue as IntPropertyValue;
    return propertyValue.value;
  });
}
