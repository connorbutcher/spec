import { Component, computed, inject, signal } from '@angular/core';
import { CurrentUserStore } from '../../../core/auth/current-user.store';
import { PERMISSIONS } from '../../../core/auth/permissions';
import { Phase } from '../../../core/models/phase.model';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { PhasesStore } from '../../phases/phases.store';
import { AddPhaseDialog } from '../add-phase-dialog/add-phase-dialog';
import { AdminPhaseTree } from '../admin-phase-tree/admin-phase-tree';
import { PhaseAvailability } from '../phase-availability/phase-availability';

/**
 * The Phases tab: the phase tree on the left, and on the right the sheet types available to the phase
 * picked in it. Adding a phase opens a dialog that starts under the picked phase.
 */
@Component({
  selector: 'app-admin-phases',
  imports: [AddPhaseDialog, AdminPhaseTree, EmptyState, PhaseAvailability],
  templateUrl: './admin-phases.html',
  styleUrl: './admin-phases.scss',
})
export class AdminPhases {
  public readonly selectedPhaseId = signal<number | null>(null);

  public readonly isAdding = signal(false);

  public readonly selectedPhase = computed<Phase | undefined>(() => {
    const id = this.selectedPhaseId();
    return id === null ? undefined : this.store.phaseById(id);
  });

  public readonly canManage = computed(() => this.currentUser.can(PERMISSIONS.phasesManage));

  private readonly store = inject(PhasesStore);
  private readonly currentUser = inject(CurrentUserStore);

  /** Shows the new phase in the tree and opens it, ready for its sheet types to be changed. */
  public onCreated(phase: Phase): void {
    this.store.reload();
    this.selectedPhaseId.set(phase.id);
  }
}
