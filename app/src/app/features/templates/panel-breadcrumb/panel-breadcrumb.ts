import { Component, computed, inject, input } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { PanelCrumb } from '../models/panel-crumb.model';
import { PanelNavigator } from '../panel-navigator';
import { StructureIcons } from '../structure-icons';

/**
 * Where the selection sits in the table, as a one-line PrimeNG breadcrumb with each level's icon.
 * Earlier steps select their part of the table.
 */
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
      // The table's name is in the toolbar, so its step is just its icon once there's a deeper one.
      label: crumb.level === 'table' && crumbs.length > 1 ? undefined : crumb.label,
      title: crumb.label,
      icon: `pi ${StructureIcons.icon(crumb.level)} ${StructureIcons.levelClass(crumb.level)}`,
      styleClass: index === crumbs.length - 1 ? 'current' : undefined,
      command: index === crumbs.length - 1 ? undefined : () => this.navigator.open(crumb.ref),
    }));
  });

  private readonly navigator = inject(PanelNavigator);
}
