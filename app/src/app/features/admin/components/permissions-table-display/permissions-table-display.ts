import { Component, computed, input } from '@angular/core';
import { Chip } from 'primeng/chip';
import { UserPermission } from '../../../../core/models/domain/access/user-permission';

@Component({
  selector: 'app-permissions-table-display',
  imports: [Chip],
  templateUrl: './permissions-table-display.html',
  styleUrl: './permissions-table-display.scss',
})
export class PermissionsTableDisplay {
  public readonly permissions = input.required<UserPermission[]>();
  public readonly displayablePermissions = computed(() =>
    this.permissions().filter((_, i) => i < 3),
  );
}
