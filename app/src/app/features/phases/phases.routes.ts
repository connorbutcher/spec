import { Routes } from '@angular/router';

export const PHASES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./phases-page/phases-page').then((m) => m.PhasesPage),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./phase-select-prompt/phase-select-prompt').then((m) => m.PhaseSelectPrompt),
      },
      {
        path: ':phaseId',
        loadComponent: () => import('./phase-detail/phase-detail').then((m) => m.PhaseDetail),
      },
    ],
  },
];
