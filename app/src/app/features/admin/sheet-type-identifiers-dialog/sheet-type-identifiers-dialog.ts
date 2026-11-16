import { Component, computed, inject, input, linkedSignal, model, output, signal } from '@angular/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { SheetType } from '../../../core/models/sheet-type.model';
import { SheetTypeIdentifier } from '../../../core/models/sheet-type-identifier.model';
import { AddButton } from '../../../shared/components/add-button/add-button';
import { ErrorMessage } from '../../../shared/components/error-message/error-message';
import { ApiErrorMessage } from '../../templates/api-error-message';
import { AdminApi } from '../admin-api';
import { IdentifierForm } from '../identifier-form/identifier-form';

/**
 * A sheet type's identifiers: the values that tell its sheets apart within a phase, which each new sheet
 * is asked for. Adding, changing and removing them. One that sheets already have a value for can't be
 * removed or change kind.
 */
@Component({
  selector: 'app-sheet-type-identifiers-dialog',
  imports: [AddButton, ButtonModule, ConfirmPopupModule, DialogModule, ErrorMessage, IdentifierForm, TableModule],
  providers: [ConfirmationService],
  templateUrl: './sheet-type-identifiers-dialog.html',
  styleUrl: './sheet-type-identifiers-dialog.scss',
})

export class SheetTypeIdentifiersDialog {
  public readonly visible = model(false);
  public readonly sheetType = input<SheetType | null>(null);

  /** The sheet type changed; the caller reloads its list. */
  public readonly changed = output<SheetType>();

  /** The sheet type as last saved here, so each change shows at once. */
  public readonly current = linkedSignal(() => this.sheetType());

  /** The identifier being changed, 'new' while one is being added, or null while neither. */
  public readonly editing = signal<SheetTypeIdentifier | 'new' | null>(null);
  public readonly error = signal<string | null>(null);

  public readonly dialogTitle = computed(() => `${this.current()?.name ?? 'Sheet type'} identifiers`);

  public readonly identifiers = computed(() => this.current()?.identifiers ?? []);

  /** At most four, so a sheet's values always fit its identity key. */
  public readonly canAdd = computed(() => this.identifiers().length < 4 && this.editing() === null);

  public readonly editedIdentifier = computed(() => {
    const editing = this.editing();
    return editing === 'new' ? null : editing;
  });

  private readonly api = inject(AdminApi);
  private readonly confirmation = inject(ConfirmationService);

  public reset(): void {
    this.editing.set(null);
    this.error.set(null);
  }

  public kindLabel(identifier: SheetTypeIdentifier): string {
    return identifier.kind === 'Option' ? `Dropdown: ${identifier.options.join(', ')}` : identifier.kind;
  }

  public onSaved(sheetType: SheetType): void {
    this.current.set(sheetType);
    this.editing.set(null);
    this.changed.emit(sheetType);
  }

  public askRemove(event: Event, identifier: SheetTypeIdentifier): void {
    this.confirmation.confirm({
      target: event.currentTarget as EventTarget,
      message: `Remove ${identifier.name}? Only an identifier no sheet has a value for can be removed.`,
      acceptLabel: 'Remove',
      rejectLabel: 'Cancel',
      acceptButtonProps: { size: 'small', severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', size: 'small', outlined: true },
      accept: () => void this.remove(identifier),
    });
  }

  private async remove(identifier: SheetTypeIdentifier): Promise<void> {
    const sheetType = this.current();
    if (sheetType === null) {
      return;
    }
    this.error.set(null);
    try {
      this.onSaved(await this.api.deleteIdentifier(sheetType.id, identifier.id));
    } catch (error) {
      this.error.set(ApiErrorMessage.apiErrorMessage(error));
    }
  }
}
