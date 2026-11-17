import { Component, input, linkedSignal } from '@angular/core';
import { Tooltip } from 'primeng/tooltip';

@Component({
  selector: 'app-element-label',
  templateUrl: './element-label.html',
  styleUrl: './element-label.scss',
  imports: [Tooltip],
})
export class ElementLabel {
  public readonly labelText = input.required<string>();
  public readonly infoText = input<string>();
  public readonly enableExpansion = input<boolean>(false);
  public readonly isExpandedDefault = input<boolean>(true);

  public readonly isExpanded = linkedSignal<boolean>(() =>
    this.isExpandedDefault(),
  );

  public toggleExpansion(): void {
    this.isExpanded.set(!this.isExpanded());
  }
}
