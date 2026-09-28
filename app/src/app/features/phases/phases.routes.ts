import { Routes } from '@angular/router';

export const PHASES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./phases-page/phases-page.component').then((m) => m.PhasesPageComponent),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./phase-select-prompt/phase-select-prompt.component').then(
            (m) => m.PhaseSelectPromptComponent,
          ),
      },
      {
        path: ':phaseId',
        loadComponent: () =>
          import('./phase-detail/phase-detail.component').then((m) => m.PhaseDetailComponent),
      },
    ],
  },
];
