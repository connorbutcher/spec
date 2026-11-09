import { Routes } from '@angular/router';

export class AppRoutes {
  public static comingSoon() {
    return import('./features/coming-soon/coming-soon-page/coming-soon-page').then((m) => m.ComingSoonPage);
  }
}

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'phases' },
  {
    path: 'phases',
    title: 'Phases · PU Spec Sheet',
    loadChildren: () => import('./features/phases/phases.routes').then((m) => m.PHASES_ROUTES),
  },
  {
    path: 'approvals',
    title: 'Approvals · PU Spec Sheet',
    loadChildren: () => import('./features/approvals/approvals.routes').then((m) => m.APPROVALS_ROUTES),
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
    loadComponent: AppRoutes.comingSoon,
    data: { title: 'Help', icon: 'pi-question-circle' },
  },
  { path: '**', redirectTo: 'phases' },
];
