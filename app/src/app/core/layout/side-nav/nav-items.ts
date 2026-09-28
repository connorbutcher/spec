import { NavItem } from './nav-item.model';

/** The main sections of the app, in the order they appear in the side nav. */
export const MAIN_NAV_ITEMS: readonly NavItem[] = [
  { label: 'Phases', icon: 'pi-sitemap', path: '/phases' },
  { label: 'Templates', icon: 'pi-table', path: '/templates' },
  { label: 'Admin', icon: 'pi-cog', path: '/admin' },
];

/** Secondary entries pinned to the bottom of the side nav. */
export const FOOTER_NAV_ITEMS: readonly NavItem[] = [
  { label: 'Help', icon: 'pi-question-circle', path: '/help' },
];
