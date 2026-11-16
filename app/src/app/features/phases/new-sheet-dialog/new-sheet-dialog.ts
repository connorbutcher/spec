import { Component, computed, inject, input, model, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { Phase } from '../../../core/models/phase.model';
import { SheetSummary } from '../../../core/models/sheet-summary.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { DialogActions } from '../../../shared/components/dialog-actions/dialog-actions';
import { ErrorMessage } from '../../../shared/components/error-message/error-message';
import { SheetIdentityUtil } from '../../../shared/sheet-identity.util';
import { ApiErrorMessage } from '../../templates/api-error-message';
import { IdentifierValueField } from '../identifier-value-field/identifier-value-field';
import { CopySourceOption } from '../models/copy-source-option.model';
import { PhaseSheetsApi } from '../phase-sheets-api';

/**
 * The dialog that creates a sheet of a sheet type in a phase: a value for each of the type's identifiers,
 * fixed from then on, and optionally another sheet of the type to start as a copy of.
 */
@Component({
  selector: 'app-new-sheet-dialog',
  imports: [DialogActions, DialogModule, ErrorMessage, FormsModule, IdentifierValueField, SelectModule],
  templateUrl: './new-sheet-dialog.html',
  styleUrl: './new-sheet-dialog.scss',
})

export class NewSheetDialog {
  public readonly visible = model(false);
  public readonly phase = input.required<Phase>();
  public readonly sheetType = input<SheetType | null>(null);
  /** The phase's sheets of the type, which the new one can start as a copy of. */
  public readonly sheets = input<SheetSummary[]>([]);

  public readonly created = output<SheetSummary>();

  public readonly values = signal<Readonly<Record<number, string>>>({});
  public readonly copyFromSheetId = signal<number | null>(null);
  public readonly error = signal<string | null>(null);
  public readonly isSaving = signal(false);

  public readonly header = computed(() => `New ${this.sheetType()?.name ?? 'sheet'} for ${this.phase().code}`);

  public readonly identifiers = computed(() => this.sheetType()?.identifiers ?? []);

  public readonly copySources = computed<CopySourceOption[]>(() => {
    const sheetType = this.sheetType();
    return sheetType === null
      ? []
      : this.sheets().map((sheet) => ({ id: sheet.id, label: SheetIdentityUtil.label(sheet, sheetType) }));
  });

  public readonly canCreate = computed(
    () =>
      !this.isSaving() &&
      this.identifiers().every((identifier) => !identifier.isRequired || (this.values()[identifier.id] ?? '').trim() !== ''),
  );

  private readonly api = inject(PhaseSheetsApi);

  /** Clears the form each time the dialog opens. */
  public reset(): void {
    this.values.set({});
    this.copyFromSheetId.set(null);
    this.error.set(null);
  }

  public valueOf(identifierId: number): string {
    return this.values()[identifierId] ?? '';
  }

  public setValue(identifierId: number, value: string): void {
    this.values.update((values) => ({ ...values, [identifierId]: value }));
  }

  public async create(): Promise<void> {
    const sheetType = this.sheetType();
    if (sheetType === null || !this.canCreate()) {
      return;
    }

    this.isSaving.set(true);
    this.error.set(null);
    try {
      const sheet = await this.api.create(this.phase().id, {
        sheetTypeId: sheetType.id,
        identifiers: this.identifiers().map((identifier) => ({ identifierId: identifier.id, value: this.valueOf(identifier.id).trim() })),
        copyFromSheetId: this.copyFromSheetId(),
      });
      this.visible.set(false);
      this.created.emit(sheet);
    } catch (error) {
      this.error.set(ApiErrorMessage.apiErrorMessage(error));
    } finally {
      this.isSaving.set(false);
    }
  }
}
