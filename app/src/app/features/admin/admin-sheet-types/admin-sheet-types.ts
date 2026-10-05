import { Component, computed, inject, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { CurrentUserStore } from '../../../core/auth/current-user.store';
import { PERMISSIONS } from '../../../core/auth/permissions';
import { sheetTypeIcon } from '../../../shared/sheet-type-icon';
import { PhasesStore } from '../../phases/phases.store';
import { AddSheetTypeDialog } from '../add-sheet-type-dialog/add-sheet-type-dialog';
import { sheetTypeUsage } from '../admin.util';
import { SheetTypeUsage } from '../models/sheet-type-usage.model';

/** The Sheet types tab: every sheet type with the phases that have it, and the button that adds one. */
@Component({
  selector: 'app-admin-sheet-types',
  imports: [AddSheetTypeDialog, ButtonModule, TableModule, TooltipModule],
  templateUrl: './admin-sheet-types.html',
  styleUrl: './admin-sheet-types.scss',
})
export class AdminSheetTypes {
  public readonly isAdding = signal(false);

  public readonly rows = computed<SheetTypeUsage[]>(() =>
    sheetTypeUsage(this.store.sheetTypes(), this.store.tree()),
  );

  public readonly canManage = computed(() => this.currentUser.can(PERMISSIONS.sheetTypesManage));

  private readonly store = inject(PhasesStore);
  private readonly currentUser = inject(CurrentUserStore);

  public icon(row: SheetTypeUsage): string {
    return sheetTypeIcon(row.sheetType.name);
  }

  public onCreated(): void {
    this.store.reload();
  }
}
