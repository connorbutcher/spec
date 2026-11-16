import { Component, computed, inject, input, linkedSignal, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { SheetIdentifierKind } from '../../../core/models/sheet-identifier-kind';
import { SheetType } from '../../../core/models/sheet-type.model';
import { SheetTypeIdentifier } from '../../../core/models/sheet-type-identifier.model';
import { DialogActions } from '../../../shared/components/dialog-actions/dialog-actions';
import { ErrorMessage } from '../../../shared/components/error-message/error-message';
import { ApiErrorMessage } from '../../templates/api-error-message';
import { AdminApi } from '../admin-api';
import { IdentifierKindOption } from '../models/identifier-kind-option.model';

const KINDS: IdentifierKindOption[] = [
  { label: 'Text', value: 'Text' },
  { label: 'Number', value: 'Number' },
  { label: 'Date', value: 'Date' },
  { label: 'Dropdown', value: 'Option' },
];

/** Adds an identifier to a sheet type, or changes one (`identifier` set). Options are typed one per line. */
@Component({
  selector: 'app-identifier-form',
  imports: [CheckboxModule, DialogActions, ErrorMessage, FormsModule, InputTextModule, SelectModule, TextareaModule],
  templateUrl: './identifier-form.html',
  styleUrl: './identifier-form.scss',
})

export class IdentifierForm {
  public readonly sheetTypeId = input.required<number>();
  /** The identifier being changed; null to add one. */
  public readonly identifier = input<SheetTypeIdentifier | null>(null);

  public readonly saved = output<SheetType>();
  public readonly cancelled = output();

  public readonly kinds = KINDS;

  public readonly name = linkedSignal(() => this.identifier()?.name ?? '');
  public readonly kind = linkedSignal<SheetIdentifierKind>(() => this.identifier()?.kind ?? 'Text');
  public readonly isRequired = linkedSignal(() => this.identifier()?.isRequired ?? true);
  public readonly optionsText = linkedSignal(() => (this.identifier()?.options ?? []).join('\n'));
  public readonly error = signal<string | null>(null);
  public readonly isSaving = signal(false);

  public readonly confirmLabel = computed(() => (this.identifier() === null ? 'Add identifier' : 'Save'));

  public readonly options = computed(() =>
    this.optionsText()
      .split('\n')
      .map((option) => option.trim())
      .filter((option) => option !== ''),
  );

  public readonly canSave = computed(
    () => !this.isSaving() && this.name().trim() !== '' && (this.kind() !== 'Option' || this.options().length > 0),
  );

  private readonly api = inject(AdminApi);

  public async save(): Promise<void> {
    if (!this.canSave()) {
      return;
    }

    const request = { name: this.name().trim(), kind: this.kind(), isRequired: this.isRequired(), options: this.options() };
    const identifier = this.identifier();
    this.isSaving.set(true);
    this.error.set(null);
    try {
      const sheetType =
        identifier === null
          ? await this.api.addIdentifier(this.sheetTypeId(), request)
          : await this.api.updateIdentifier(this.sheetTypeId(), identifier.id, request);
      this.saved.emit(sheetType);
    } catch (error) {
      this.error.set(ApiErrorMessage.apiErrorMessage(error));
    } finally {
      this.isSaving.set(false);
    }
  }
}
