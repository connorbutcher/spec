import { Injectable, signal } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { LocalUserOption } from '../models/local-users/local-user-option';

/**
 * While auth is disabled for local running the API acts as one of three local users, picked by the X-Local-User
 * header. This keeps which one this browser acts as, so checkouts and approvals can be tried as more than one user
 * (use a normal and a private window to be two at once). It does nothing in production builds.
 */
@Injectable({
  providedIn: 'root',
})
export class LocalUserService {
  public static readonly HeaderName = 'X-Local-User';

  public readonly isEnabled = !environment.production;
  public readonly options: LocalUserOption[] = [
    { label: 'Local Admin', value: 1 },
    { label: 'Local Engineer', value: 2 },
    { label: 'Local Approver', value: 3 },
  ];
  public readonly selected = signal<number>(this.readSelected());

  private static readonly StorageKey = 'localUser';

  /** Acts as another local user from now on; the app is reloaded so everything is read as them. */
  public select(value: number): void {
    if (!this.isEnabled || value === this.selected()) {
      return;
    }

    sessionStorage.setItem(LocalUserService.StorageKey, String(value));
    window.location.reload();
  }

  private readSelected(): number {
    const stored = Number(sessionStorage.getItem(LocalUserService.StorageKey));
    return this.options.some((option) => option.value === stored) ? stored : 1;
  }
}
