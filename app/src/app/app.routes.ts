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
    loadChildren: () =>
      import('./features/templates/templates.routes').then((m) => m.TEMPLATES_ROUTES),
  },
  {
    path: 'admin',
    title: 'Admin · PU Spec Sheet',
    loadChildren: () => import('./features/admin/admin.routes').then((m) => m.ADMIN_ROUTES),
  },
  {
    path: 'help',
    title: 'Help · PU Spec Sheet',
    loadComponent: comingSoon,
    data: { title: 'Help', icon: 'pi-question-circle' },
  },
  { path: '**', redirectTo: 'phases' },
];
