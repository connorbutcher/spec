import { Routes } from '@angular/router';

export const PHASES_ROUTES: Routes = [
  {
    // A phase's sheet opens on its own full-width screen, outside the phase tree layout.
    path: ':phaseId/sheets/:sheetTypeId',
    title: 'Sheet · PU Spec Sheet',
    loadComponent: () => import('../sheets/sheet-page/sheet-page').then((m) => m.SheetPage),
  },
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
