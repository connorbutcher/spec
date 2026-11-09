import { NavItem } from './nav-item.model';

/** The main sections of the app, in the order they appear in the side nav. */
export const MAIN_NAV_ITEMS: readonly NavItem[] = [
  { label: 'Phases', icon: 'pi-sitemap', path: '/phases' },
  { label: 'Templates', icon: 'pi-table', path: '/templates' },
  { label: 'Admin', icon: 'pi-cog', path: '/admin' },
];

/**
 * Entries pinned to the bottom of the side nav. Approvals is here so its count of changes waiting for the
 * user is in view on every screen, however long the phase tree above it grows.
 */
export const FOOTER_NAV_ITEMS: readonly NavItem[] = [
  { label: 'Approvals', icon: 'pi-check-square', path: '/approvals' },
  { label: 'Help', icon: 'pi-question-circle', path: '/help' },
];
