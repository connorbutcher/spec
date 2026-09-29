import { Component, computed, input } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { BreadcrumbItem } from './breadcrumb-item.model';

/** A compact PrimeNG breadcrumb trail. Items with a link navigate; the last item is the current page. */
@Component({
  selector: 'app-breadcrumb',
  imports: [BreadcrumbModule],
  templateUrl: './breadcrumb.html',
  styleUrl: './breadcrumb.scss',
})
export class Breadcrumb {
  public readonly items = input.required<BreadcrumbItem[]>();

  public readonly menuItems = computed<MenuItem[]>(() =>
    this.items().map((item, index, all) => {
      const isLast = index === all.length - 1;
      return isLast || !item.link
        ? { label: item.label, styleClass: isLast ? 'current' : undefined }
        : { label: item.label, routerLink: item.link };
    }),
  );
}
