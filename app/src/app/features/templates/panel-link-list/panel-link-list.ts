import { Component, inject, input } from '@angular/core';
import { PanelLinkItem } from '../models/panel-link-item.model';
import { PanelNavigator } from '../panel-navigator';

/** A compact list of child items; clicking one opens it in the panel. */
@Component({
  selector: 'app-panel-link-list',
  templateUrl: './panel-link-list.html',
  styleUrl: './panel-link-list.scss',
})
export class PanelLinkList {
  public readonly items = input.required<PanelLinkItem[]>();
  public readonly emptyText = input('Nothing here yet.');

  private readonly navigator = inject(PanelNavigator);

  public open(item: PanelLinkItem): void {
    this.navigator.open(item.ref);
  }
}
