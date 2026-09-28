import { Component, inject, input } from '@angular/core';
import { PanelCrumb } from '../models/panel-crumb.model';
import { PanelNavigator } from '../panel-navigator';

/** Where the open panel sits in the table. Earlier steps open their own panels. */
@Component({
  selector: 'app-panel-breadcrumb',
  templateUrl: './panel-breadcrumb.html',
  styleUrl: './panel-breadcrumb.scss',
})
export class PanelBreadcrumb {
  public readonly crumbs = input.required<PanelCrumb[]>();

  private readonly navigator = inject(PanelNavigator);

  public open(crumb: PanelCrumb): void {
    this.navigator.open(crumb.ref);
  }
}
