import { Component, computed, input } from '@angular/core';
import { Tooltip } from 'primeng/tooltip';
import { SheetPropertyHelpers } from '../../../../../core/helpers/sheet-property-helpers';
import { SheetProperty } from '../../../../../core/models/domain/sheets/sheet-property';

@Component({
  selector: 'app-number-display-property-control',
  templateUrl: './number-display-property-control.html',
  styleUrl: './number-display-property-control.scss',
  imports: [Tooltip],
})
export class NumberDisplayPropertyControl {
  public readonly property = input.required<SheetProperty>();
  public readonly displayValue = computed(() => {
    const instancePropertyValue = this.property();
    if (!instancePropertyValue) {
      return 'N/A';
    }

    return (
      SheetPropertyHelpers.getValue<number>(instancePropertyValue) ?? 'N/A'
    );
  });
}
