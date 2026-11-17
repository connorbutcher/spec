import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserService } from '../services/user.service';

export const userRegisteredGuard: CanActivateFn = (route, state) => {
  const userService = inject(UserService);
  const router = inject(Router);

  if (!userService.user()) {
    router.navigate([`/login/oauth-callback`], {
      queryParams: { postLoginRoute: encodeURIComponent(state.url) },
      skipLocationChange: true,
    });
    return false;
  }

  return true;
};
