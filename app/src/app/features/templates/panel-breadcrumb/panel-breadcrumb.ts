import { Component, computed, inject, input } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { PanelCrumb } from '../models/panel-crumb.model';
import { PanelNavigator } from '../panel-navigator';

/** Where the open panel sits in the table, as a PrimeNG breadcrumb. Earlier steps open their panels. */
@Component({
  selector: 'app-panel-breadcrumb',
  imports: [BreadcrumbModule],
  templateUrl: './panel-breadcrumb.html',
  styleUrl: './panel-breadcrumb.scss',
})
export class PanelBreadcrumb {
  public readonly crumbs = input.required<PanelCrumb[]>();

  public readonly items = computed<MenuItem[]>(() => {
    const crumbs = this.crumbs();
    return crumbs.map((crumb, index) => ({
      label: crumb.label,
      styleClass: index === crumbs.length - 1 ? 'current' : undefined,
      command: index === crumbs.length - 1 ? undefined : () => this.navigator.open(crumb.ref),
    }));
  });

  private readonly navigator = inject(PanelNavigator);
}
