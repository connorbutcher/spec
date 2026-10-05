import { httpResource } from '@angular/common/http';
import { computed, Service } from '@angular/core';
import { CurrentUser } from '../models/current-user.model';
import { Permission } from './permissions';

/**
 * The current user and what they are allowed to do, fetched once for the whole app. Screens use
 * `can()` to hide or disable what the user's roles don't allow; the API refuses those requests anyway.
 */
@Service()
export class CurrentUserStore {
  public readonly user = computed<CurrentUser | null>(() =>
    this.userResource.hasValue() ? this.userResource.value() : null,
  );

  public readonly isLoading = computed(() => this.userResource.isLoading());

  public readonly hasError = computed(() => this.userResource.status() === 'error');

  private readonly userResource = httpResource<CurrentUser>(() => '/api/me');

  /** Whether the user holds a permission. False until the user has loaded. */
  public can(permission: Permission): boolean {
    const user = this.user();
    return user !== null && (user.isAdministrator || user.permissions.includes(permission));
  }

  public reload(): void {
    this.userResource.reload();
  }
}
