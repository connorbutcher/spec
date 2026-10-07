import { ActivatedRouteSnapshot } from '@angular/router';

/** Finds a route parameter anywhere down the active route tree, or null when it isn't in the URL. */
export function findRouteParam(route: ActivatedRouteSnapshot, name: string): string | null {
  let current: ActivatedRouteSnapshot | null = route;
  while (current) {
    const value = current.paramMap.get(name);
    if (value !== null) {
      return value;
    }
    current = current.firstChild;
  }
  return null;
}
