import {
  Component,
  effect,
  inject,
  model,
  signal,
  Signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { InputText } from 'primeng/inputtext';
import { User } from '../../../../../../core/models/domain/access/user';
import { AdminService } from '../../../../admin.service';
import { PermissionsService } from '../../services/permissions.service';

@Component({
  selector: 'app-user-list',
  imports: [FormsModule, InputText],
  templateUrl: './user-list.html',
  styleUrl: './user-list.scss',
})
export class UserList {
  public readonly filteredUsers = signal<User[]>([]);
  public readonly filterValue = model<string>('');

  private readonly _adminService = inject(AdminService);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _permissionsService = inject(PermissionsService);

  public readonly selectedUser = model<User | undefined>();

  constructor() {
    effect(() => {
      const activeMode = this._permissionsService.activeMode();
      if (activeMode === 'byuser') {
        const user = this._adminService
          .users()
          .find((user) => user.id === this._permissionsService.activeId());
        if (user) {
          this.selectedUser.set(user);
        }
      } else {
        this.selectedUser.set(undefined);
      }
    });

    effect(() => {
      const filterValue = this.filterValue().trim().toLocaleLowerCase();
      const users = this._adminService.users();

      if (!filterValue) {
        this.filteredUsers.set(users);
        return;
      }

      this.filteredUsers.set(
        users.filter((user) => {
          return (
            user.displayName.toLocaleLowerCase().includes(filterValue) ||
            user.emailAddress.toLocaleLowerCase().includes(filterValue)
          );
        }),
      );
    });
  }

  public selectUser(user: User) {
    if (!user) {
      return;
    }

    this.selectedUser.set(user);
    this._router.navigate(['user', user.id], {
      relativeTo: this._activatedRoute,
    });
  }

  public get users(): Signal<User[]> {
    return this._adminService.users;
  }
}
