import { Component, computed, input } from '@angular/core';
import { Tooltip } from 'primeng/tooltip';
import { SheetPropertyHelpers } from '../../../../../core/helpers/sheet-property-helpers';
import { SheetProperty } from '../../../../../core/models/domain/sheets/sheet-property';

@Component({
  selector: 'app-text-display-property-control',
  templateUrl: './text-display-property-control.html',
  styleUrl: './text-display-property-control.scss',
  imports: [Tooltip],
})
export class TextDisplayPropertyControl {
  public readonly property = input.required<SheetProperty>();
  public readonly displayValue = computed(() => {
    const property = this.property();
    if (!property) {
      return 'N/A';
    }

    return SheetPropertyHelpers.getText(property, 'N/A');
  });
  public readonly classes = input<string>();
}
