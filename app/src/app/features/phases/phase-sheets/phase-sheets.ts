import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CurrentUserStore } from '../../../core/auth/current-user.store';
import { PERMISSIONS } from '../../../core/auth/permissions';
import { Phase } from '../../../core/models/phase.model';
import { SheetSummary } from '../../../core/models/sheet-summary.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { SheetIdentityUtil } from '../../../shared/sheet-identity.util';
import { NewSheetDialog } from '../new-sheet-dialog/new-sheet-dialog';
import { PhaseSheetGroup } from '../phase-sheet-group/phase-sheet-group';

/** The phase's PU Spec Sheets, a table per sheet type, and the dialog that creates one. */
@Component({
  selector: 'app-phase-sheets',
  imports: [ButtonModule, NewSheetDialog, PhaseSheetGroup],
  templateUrl: './phase-sheets.html',
  styleUrl: './phase-sheets.scss',
})

export class PhaseSheets {
  public readonly phase = input.required<Phase>();
  public readonly sheetTypes = input.required<SheetType[]>();

  public readonly creatingType = signal<SheetType | null>(null);
  public readonly isCreating = signal(false);

  public readonly sheets = computed<SheetSummary[]>(() =>
    this.sheetsResource.hasValue() ? this.sheetsResource.value() : [],
  );

  public readonly hasError = computed(() => this.sheetsResource.status() === 'error');

  /** The phase's sheets of the type being created, which the new one can start as a copy of. */
  public readonly copySources = computed<SheetSummary[]>(() => {
    const sheetType = this.creatingType();
    return sheetType === null ? [] : this.sheets().filter((sheet) => sheet.sheetTypeId === sheetType.id);
  });

  public readonly canCreate = computed(() => this.currentUser.can(PERMISSIONS.sheetsEdit));

  private readonly currentUser = inject(CurrentUserStore);
  private readonly router = inject(Router);

  private readonly sheetsResource = httpResource<SheetSummary[]>(() => `/api/phases/${this.phase().id}/sheets`);

  public startCreating(sheetType: SheetType): void {
    this.creatingType.set(sheetType);
    this.isCreating.set(true);
  }

  public onCreated(sheet: SheetSummary): void {
    this.sheetsResource.reload();
    void this.router.navigate(SheetIdentityUtil.link(sheet));
  }

  public retry(): void {
    this.sheetsResource.reload();
  }
}
