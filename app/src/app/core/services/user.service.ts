import { inject, Injectable, signal } from '@angular/core';
import { User } from '../models/domain/access/user';
import { ConditionalAccessService } from './conditional-access.service';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  public readonly user = signal<User | undefined>(undefined);

  private readonly _conditionalAccessService = inject(ConditionalAccessService);

  public setUser(user: User): void {
    this.user.set(user);
    this._conditionalAccessService.set(
      ...user.permissions.map((p) => p.permission.key),
    );
  }
}
