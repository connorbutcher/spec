import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { ConditionalAccessService } from '../services/conditional-access.service';

export const userRoleGuard: CanActivateFn = (route, state) => {
  const conditionalAccessService = inject(ConditionalAccessService);

  const roles = route.data['roles'] as string[];
  return conditionalAccessService.has(roles);
};
