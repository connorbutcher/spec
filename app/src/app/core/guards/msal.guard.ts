import { inject } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  GuardResult,
  MaybeAsync,
  RouterStateSnapshot,
} from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';

export function msalGuard(): CanActivateFn {
  return (
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot,
  ): MaybeAsync<GuardResult> => {
    const msalGuard = inject(MsalGuard);
    return msalGuard.canActivate(route, state);
  };
}
