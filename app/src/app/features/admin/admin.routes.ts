import { Routes } from '@angular/router';
// import { MsalGuard } from '@azure/msal-angular';
import { userRegisteredGuard } from '../../core/guards/user-registered.guard';
// import { userRoleGuard } from '../../core/guards/user-role.guard';

export const adminRoles = ['admin.view'];

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./admin').then((m) => m.Admin),
    canActivate: [/* MsalGuard, */ userRegisteredGuard /* , userRoleGuard */],
    data: { roles: adminRoles },
    title: 'Admin',
    children: [
      {
        path: 'sheets',
        loadComponent: () =>
          import('./features/sheets-audit/sheets-audit').then(
            (m) => m.SheetsAudit,
          ),
        canActivate: [/* MsalGuard, */ userRegisteredGuard],
        data: { roles: ['admin.sheets'] },
        title: 'Admin - Sheets',
      },
      {
        path: 'users',
        loadComponent: () => import('./features/users').then((m) => m.Users),
        canActivate: [/* MsalGuard, */ userRegisteredGuard],
        data: { roles: ['admin.users'] },
        title: 'Admin - Users',
      },
      {
        path: 'phases',
        loadComponent: () =>
          import('./features/phases/phases').then((m) => m.Phases),
        canActivate: [/* MsalGuard, */ userRegisteredGuard],
        data: { roles: ['admin.phases'] },
        title: 'Admin - Phases',
        children: [
          {
            path: 'create',
            loadComponent: () =>
              import('./features/phases/components/create-phase/create-phase').then(
                (m) => m.CreatePhase,
              ),
            canActivate: [/* MsalGuard, */ userRegisteredGuard],
          },
          {
            path: ':id',
            loadComponent: () =>
              import('./features/phases/components/view-phase/view-phase').then(
                (m) => m.ViewPhase,
              ),
            canActivate: [/* MsalGuard, */ userRegisteredGuard],
          },
        ],
      },
      {
        path: 'types',
        loadComponent: () =>
          import('./features/types/types').then((m) => m.Types),
        canActivate: [/* MsalGuard, */ userRegisteredGuard],
        data: { roles: ['admin.types'] },
        title: 'Admin - Types',
      },
      {
        path: 'permissions',
        loadComponent: () =>
          import('./features/permissions/permissions').then(
            (m) => m.Permissions,
          ),
        canActivate: [/* MsalGuard, */ userRegisteredGuard],
        data: { roles: ['admin.permissions'] },
        title: 'Admin - Permissions',
        children: [
          {
            path: 'user/:id',
            loadComponent: () =>
              import('./features/permissions/components/permissions-by-user/permissions-by-user').then(
                (m) => m.PermissionsByUser,
              ),
            canActivate: [/* MsalGuard, */ userRegisteredGuard],
          },
          {
            path: 'type/:id',
            loadComponent: () =>
              import('./features/permissions/components/permissions-by-spec-sheet-type/permissions-by-spec-sheet-type').then(
                (m) => m.PermissionsBySpecSheetType,
              ),
            canActivate: [/* MsalGuard, */ userRegisteredGuard],
          },
        ],
      },
    ],
  },
];
