import { Routes } from '@angular/router';
import { chainActivateGuards } from './core/guards/guard-helpers';
import { legacySpecSheetGuard } from './core/guards/legacy-spec-sheet.guard';
// import { msalGuard } from './core/guards/msal.guard';
import { userRegisteredGuard } from './core/guards/user-registered.guard';
// import { userRoleGuard } from './core/guards/user-role.guard';
import { adminRoles } from './features/admin/admin.routes';
import { SheetDataService } from './features/sheet/services/sheet-data.service';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'phases' },
  {
    path: 'spec/:id',
    loadComponent: () => import('./features/sheet/sheet').then((m) => m.Sheet),
    providers: [SheetDataService],
    canActivate: [
      chainActivateGuards(
        // msalGuard(),
        userRegisteredGuard,
        legacySpecSheetGuard,
      ),
    ],
  },
  {
    path: 'phases',
    loadChildren: () =>
      import('./features/sheets/sheet.routes').then((m) => m.SHEET_ROUTES),
    canActivate: [chainActivateGuards(/* msalGuard(), */ userRegisteredGuard)],
  },
  {
    path: 'comparison',
    loadComponent: () =>
      import('./features/sheet-comparison/sheet-comparison').then(
        (m) => m.SheetComparison,
      ),
    canActivate: [chainActivateGuards(/* msalGuard(), */ userRegisteredGuard)],
    title: 'Comparison',
  },
  {
    path: 'admin',
    loadChildren: () =>
      import('./features/admin/admin.routes').then((m) => m.routes),
    canActivate: [
      chainActivateGuards(
        /* msalGuard(), */ userRegisteredGuard /* , userRoleGuard */,
      ),
    ],
    data: { roles: adminRoles },
  },
  {
    path: 'login/oauth-callback',
    loadComponent: () =>
      import('./features/login/oauth-callback/oauth-callback').then(
        (m) => m.OAuthCallback,
      ),
  },
  {
    path: 'login-failed',
    loadComponent: () =>
      import('./features/login/login-failed/login-failed').then(
        (m) => m.LoginFailed,
      ),
  },
  {
    path: '**',
    redirectTo: 'phases',
  },
];
