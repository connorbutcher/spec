import { Component, inject, input } from '@angular/core';
import { ListboxClickEvent, ListboxModule } from 'primeng/listbox';
import { PanelLinkItem } from '../models/panel-link-item.model';
import { PanelNavigator } from '../panel-navigator';

/** A compact PrimeNG list of child items; clicking one opens it in the panel. */
@Component({
  selector: 'app-panel-link-list',
  imports: [ListboxModule],
  templateUrl: './panel-link-list.html',
  styleUrl: './panel-link-list.scss',
})
export class PanelLinkList {
  public readonly items = input.required<PanelLinkItem[]>();
  public readonly emptyText = input('Nothing here yet.');
  public readonly ariaLabel = input('Items');

  private readonly navigator = inject(PanelNavigator);

  public open(event: ListboxClickEvent): void {
    const item = event.option as PanelLinkItem | undefined;
    if (item) {
      this.navigator.open(item.ref);
    }
  }
}
