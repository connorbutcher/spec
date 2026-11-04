/** One step in a breadcrumb. The last item is the current page and has no link. */
export type BreadcrumbItem = {
  label: string;
  link?: (string | number)[];
}
