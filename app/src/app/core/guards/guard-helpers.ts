import { inject, Injector, runInInjectionContext } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  GuardResult,
  MaybeAsync,
  RouterStateSnapshot,
} from '@angular/router';
import { from, isObservable, Observable, of, switchMap } from 'rxjs';

export function chainActivateGuards(...guards: CanActivateFn[]): CanActivateFn {
  return (
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot,
  ): MaybeAsync<GuardResult> => {
    return chainGuards((guard) => guard(route, state), guards);
  };
}

function chainGuards<T>(
  applyGuard: (guard: T) => MaybeAsync<GuardResult>,
  guards: T[],
): MaybeAsync<GuardResult> {
  const injectionContext = inject(Injector);

  return guards.reduce<Observable<GuardResult>>(
    (acc, guard) =>
      acc.pipe(
        switchMap((result) =>
          result === true
            ? runInInjectionContext(injectionContext, () =>
                toObservable(applyGuard(guard)),
              )
            : of(result),
        ),
      ),
    of<GuardResult>(true),
  );
}

function toObservable<T>(input: MaybeAsync<T>) {
  return isObservable(input)
    ? input
    : input instanceof Promise
      ? from(input)
      : of(input);
}
