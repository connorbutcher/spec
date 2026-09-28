import { Routes } from '@angular/router';

const comingSoon = () =>
  import('./features/coming-soon/coming-soon-page/coming-soon-page').then((m) => m.ComingSoonPage);

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'phases' },
  {
    path: 'phases',
    title: 'Phases · PU Spec Sheet',
    loadChildren: () => import('./features/phases/phases.routes').then((m) => m.PHASES_ROUTES),
  },
  {
    path: 'templates',
    title: 'Templates · PU Spec Sheet',
    loadComponent: comingSoon,
    data: { title: 'Templates', icon: 'pi-table' },
  },
  {
    path: 'admin',
    title: 'Admin · PU Spec Sheet',
    loadComponent: comingSoon,
    data: { title: 'Admin', icon: 'pi-cog' },
  },
  {
    path: 'help',
    title: 'Help · PU Spec Sheet',
    loadComponent: comingSoon,
    data: { title: 'Help', icon: 'pi-question-circle' },
  },
  { path: '**', redirectTo: 'phases' },
];
