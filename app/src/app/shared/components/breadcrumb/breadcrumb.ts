import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BreadcrumbItem } from './breadcrumb-item.model';

/** A compact breadcrumb trail. Items with a link navigate; the last item is marked as the current page. */
@Component({
  selector: 'app-breadcrumb',
  imports: [RouterLink],
  templateUrl: './breadcrumb.html',
  styleUrl: './breadcrumb.scss',
})
export class Breadcrumb {
  public readonly items = input.required<BreadcrumbItem[]>();
}
