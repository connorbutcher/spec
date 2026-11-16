import { Component, computed, inject, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { CurrentUserStore } from '../../../core/auth/current-user.store';
import { PERMISSIONS } from '../../../core/auth/permissions';
import { SheetType } from '../../../core/models/sheet-type.model';
import { AddButton } from '../../../shared/components/add-button/add-button';
import { SheetTypeIcon } from '../../../shared/sheet-type-icon';
import { PhasesStore } from '../../phases/phases.store';
import { AddSheetTypeDialog } from '../add-sheet-type-dialog/add-sheet-type-dialog';
import { AdminUtil } from '../admin.util';
import { SheetTypeIdentifiersDialog } from '../sheet-type-identifiers-dialog/sheet-type-identifiers-dialog';
import { SheetTypeUsage } from '../models/sheet-type-usage.model';

/** The Sheet types tab: every sheet type with the phases that have it, and the button that adds one. */
@Component({
  selector: 'app-admin-sheet-types',
  imports: [AddButton, AddSheetTypeDialog, ButtonModule, SheetTypeIdentifiersDialog, TableModule, TooltipModule],
  templateUrl: './admin-sheet-types.html',
  styleUrl: './admin-sheet-types.scss',
})

export class AdminSheetTypes {
  public readonly isAdding = signal(false);
  public readonly isEditingIdentifiers = signal(false);
  public readonly identifiersOf = signal<SheetType | null>(null);

  public readonly rows = computed<SheetTypeUsage[]>(() =>
    AdminUtil.sheetTypeUsage(this.store.sheetTypes(), this.store.tree()),
  );

  public readonly canManage = computed(() => this.currentUser.can(PERMISSIONS.sheetTypesManage));

  private readonly store = inject(PhasesStore);
  private readonly currentUser = inject(CurrentUserStore);

  public icon(row: SheetTypeUsage): string {
    return SheetTypeIcon.sheetTypeIcon(row.sheetType.name);
  }

  public identifierNames(row: SheetTypeUsage): string {
    const names = row.sheetType.identifiers.map((identifier) => identifier.name);
    return names.length === 0 ? 'None' : names.join(', ');
  }

  public editIdentifiers(row: SheetTypeUsage): void {
    this.identifiersOf.set(row.sheetType);
    this.isEditingIdentifiers.set(true);
  }

  public onCreated(): void {
    this.store.reload();
  }
}
