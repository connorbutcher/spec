import { Component, computed, inject, input, linkedSignal, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ListboxModule } from 'primeng/listbox';
import { MessageModule } from 'primeng/message';
import { Phase } from '../../../core/models/phase.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { PhasesStore } from '../../phases/phases.store';
import { apiErrorMessage } from '../../templates/api-error-message';
import { AdminApi } from '../admin-api';
import { sameIds } from '../admin.util';

/**
 * Which sheet types a phase has: a checklist of every sheet type, saved as a whole. Nothing is
 * inherited from the parent phase, so each phase is ticked for itself.
 */
@Component({
  selector: 'app-phase-availability',
  imports: [ButtonModule, FormsModule, ListboxModule, MessageModule],
  templateUrl: './phase-availability.html',
  styleUrl: './phase-availability.scss',
})
export class PhaseAvailability {
  public readonly phase = input.required<Phase>();
  public readonly canManage = input(false);

  /** The ticked sheet types. Starts again from what is saved whenever the phase changes or reloads. */
  public readonly ticked = linkedSignal<number[]>(() => [...this.phase().sheetTypeIds]);

  public readonly error = linkedSignal<Phase, string | null>({
    source: this.phase,
    computation: () => null,
  });

  public readonly isSaving = signal(false);

  public readonly sheetTypes = computed<SheetType[]>(() => this.store.sheetTypes());

  public readonly hasChanges = computed(() => !sameIds(this.ticked(), this.phase().sheetTypeIds));

  /** "V6 › 01-A2": where the phase sits in the tree. */
  public readonly path = computed(() =>
    [...this.store.ancestorsOf(this.phase().id), this.phase()]
      .map((phase) => phase.code)
      .join(' › '),
  );

  private readonly store = inject(PhasesStore);
  private readonly api = inject(AdminApi);

  public async save(): Promise<void> {
    this.isSaving.set(true);
    this.error.set(null);
    try {
      await this.api.setPhaseSheetTypes(this.phase().id, this.ticked());
      this.store.reload();
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.isSaving.set(false);
    }
  }

  public reset(): void {
    this.ticked.set([...this.phase().sheetTypeIds]);
    this.error.set(null);
  }
}
