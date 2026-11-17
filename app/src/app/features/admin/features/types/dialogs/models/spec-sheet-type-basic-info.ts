import { computed, signal, WritableSignal } from '@angular/core';
import { SpecSheetPhase } from '../../../../../../core/models/domain/types/spec-sheet-phase';
import {
  SpecSheetType,
  TableOrientation,
} from '../../../../../../core/models/domain/types/spec-sheet-type';

export class SpecSheetTypeBasicInfo {
  public readonly id: number;
  public readonly title: WritableSignal<string>;
  public readonly tableOrientation: WritableSignal<TableOrientation>;
  public readonly canCopyFromMigrated: WritableSignal<boolean>;
  public readonly requiresApprovalWorkflow: WritableSignal<boolean>;
  public readonly phases: WritableSignal<SpecSheetPhase[]>;
  public readonly originalPhases: WritableSignal<SpecSheetPhase[]>;

  public readonly isValid = computed(() => {
    return (
      this.title().trim().length > 0 &&
      this.phases().length > 0 &&
      this.tableOrientation() !== undefined
    );
  });
  public readonly hasChanges = computed(() => {
    const sortedOriginal = [...this.originalPhases().map((p) => p.id)].sort(
      (a, b) => a - b,
    );
    const sortedCurrent = [...this.phases().map((p) => p.id)].sort(
      (a, b) => a - b,
    );
    const phasesChanged =
      sortedOriginal.length !== sortedCurrent.length ||
      !sortedOriginal.every((phase, index) => phase === sortedCurrent[index]);

    return (
      this.title() !== this.specSheetType.type ||
      this.tableOrientation() !== this.specSheetType.tableOrientation ||
      this.canCopyFromMigrated() !== this.specSheetType.canCopyFromMigrated ||
      this.requiresApprovalWorkflow() !==
        this.specSheetType.requiresApprovalWorkflow ||
      phasesChanged
    );
  });

  constructor(public readonly specSheetType: SpecSheetType) {
    this.id = specSheetType.id;
    this.title = signal(specSheetType.type);
    this.tableOrientation = signal(specSheetType.tableOrientation);
    this.canCopyFromMigrated = signal(specSheetType.canCopyFromMigrated);
    this.requiresApprovalWorkflow = signal(
      specSheetType.requiresApprovalWorkflow,
    );
    this.phases = signal([]);
    this.originalPhases = signal([]);
  }

  public getNewPhases(): number[] {
    const originalIds = new Set(this.originalPhases().map((p) => p.id));
    return this.phases()
      .filter((p) => !originalIds.has(p.id))
      .map((p) => p.id);
  }

  public getRemovedPhases(): number[] {
    const currentIds = new Set(this.phases().map((p) => p.id));
    return this.originalPhases()
      .filter((p) => !currentIds.has(p.id))
      .map((p) => p.id);
  }
}
