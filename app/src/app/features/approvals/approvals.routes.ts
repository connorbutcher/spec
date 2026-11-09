import { Routes } from '@angular/router';

export const APPROVALS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./approvals-page/approvals-page').then((m) => m.ApprovalsPage),
  },
];
