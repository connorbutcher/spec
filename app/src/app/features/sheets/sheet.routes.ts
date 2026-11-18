import { Routes } from '@angular/router';
import { canDeactivateComponentGuard } from '../../core/guards/can-deactivate-component.guard';
import { chainActivateGuards } from '../../core/guards/guard-helpers';
// import { msalGuard } from '../../core/guards/msal.guard';
import { userRegisteredGuard } from '../../core/guards/user-registered.guard';

export const SHEET_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/phases-page/phases-page').then((m) => m.PhasesPage),
    canActivate: [chainActivateGuards(/* msalGuard(), */ userRegisteredGuard)],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./components/phase-select-prompt/phase-select-prompt').then(
            (m) => m.PhaseSelectPrompt,
          ),
        canActivate: [
          chainActivateGuards(/* msalGuard(), */ userRegisteredGuard),
        ],
      },
      {
        path: ':phaseId',
        loadComponent: () => import('./sheets').then((m) => m.Sheets),
        children: [
          {
            path: '',
            loadComponent: () =>
              import('./components/type-select-prompt/type-select-prompt').then(
                (m) => m.TypeSelectPrompt,
              ),
            canActivate: [
              chainActivateGuards(/* msalGuard(), */ userRegisteredGuard),
            ],
          },
          {
            path: 'sheets/:sheetTypeId',
            loadComponent: () => import('../sheet/sheet').then((m) => m.Sheet),
            canDeactivate: [canDeactivateComponentGuard],
            canActivate: [
              chainActivateGuards(/* msalGuard(), */ userRegisteredGuard),
            ],
          },
        ],
      },
    ],
  },
];
