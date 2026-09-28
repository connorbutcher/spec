import { Service, signal } from '@angular/core';

const COLLAPSED_KEY = 'pu-spec-sheet.side-nav-collapsed';

function readCollapsed(): boolean {
  try {
    return localStorage.getItem(COLLAPSED_KEY) === 'true';
  } catch {
    return false;
  }
}

/** App-wide layout state: whether the side nav is collapsed to icons. Remembered per browser. */
@Service()
export class LayoutService {
  public readonly sideNavCollapsed = signal(readCollapsed());

  public toggleSideNav(): void {
    this.sideNavCollapsed.update((collapsed) => !collapsed);
    try {
      localStorage.setItem(COLLAPSED_KEY, String(this.sideNavCollapsed()));
    } catch {
      // Storage can be unavailable (private mode); the toggle still works for this session.
    }
  }
}
