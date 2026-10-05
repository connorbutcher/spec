import { Component, computed, inject, input, model, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { Phase } from '../../../core/models/phase.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { PhasesStore } from '../../phases/phases.store';
import { apiErrorMessage } from '../../templates/api-error-message';
import { AdminApi } from '../admin-api';
import { phaseOptions } from '../admin.util';
import { PhaseOption } from '../models/phase-option.model';

/**
 * The dialog that adds a phase: its code, an optional description, the parent it goes under (or none,
 * for the top level) and the sheet types it has.
 */
@Component({
  selector: 'app-add-phase-dialog',
  imports: [
    ButtonModule,
    DialogModule,
    FormsModule,
    InputTextModule,
    MessageModule,
    MultiSelectModule,
    SelectModule,
  ],
  templateUrl: './add-phase-dialog.html',
  styleUrl: './add-phase-dialog.scss',
})
export class AddPhaseDialog {
  public readonly visible = model(false);

  /** The parent the dialog starts with each time it opens; null starts at the top level. */
  public readonly parentPhaseId = input<number | null>(null);

  public readonly created = output<Phase>();

  public readonly code = signal('');
  public readonly description = signal('');
  public readonly parentId = signal<number | null>(null);
  public readonly sheetTypeIds = signal<number[]>([]);
  public readonly error = signal<string | null>(null);
  public readonly isSaving = signal(false);

  public readonly parentOptions = computed<PhaseOption[]>(() => phaseOptions(this.store.tree()));

  public readonly sheetTypes = computed<SheetType[]>(() => this.store.sheetTypes());

  public readonly canAdd = computed(() => this.code().trim() !== '' && !this.isSaving());

  private readonly store = inject(PhasesStore);
  private readonly api = inject(AdminApi);

  /** Clears the form each time the dialog opens. */
  public reset(): void {
    this.code.set('');
    this.description.set('');
    this.parentId.set(this.parentPhaseId());
    this.sheetTypeIds.set([]);
    this.error.set(null);
  }

  public async add(): Promise<void> {
    if (!this.canAdd()) {
      return;
    }

    const description = this.description().trim();
    this.isSaving.set(true);
    this.error.set(null);
    try {
      const phase = await this.api.createPhase({
        code: this.code().trim(),
        description: description === '' ? null : description,
        parentPhaseId: this.parentId(),
        sheetTypeIds: this.sheetTypeIds(),
      });
      this.visible.set(false);
      this.created.emit(phase);
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.isSaving.set(false);
    }
  }
}
