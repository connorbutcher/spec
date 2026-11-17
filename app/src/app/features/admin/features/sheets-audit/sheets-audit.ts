import {
  Component,
  computed,
  effect,
  inject,
  input,
  numberAttribute,
} from '@angular/core';
import { TreeDragDropService } from 'primeng/api';
import { SpecSheetPhaseHelpers } from '../../../../core/helpers/spec-sheet-phase-helpers';
import { PhasesService } from '../../../../core/services/phases.service';
import { TypesService } from '../../../../core/services/types.service';
import { AuditSheetsForPhaseAndType } from './components/audit-sheets-for-phase-and-type/audit-sheets-for-phase-and-type';
import { PhaseSheetList } from './components/phase-sheet-list/phase-sheet-list';
import { TypeSheetList } from './components/type-sheet-list/type-sheet-list';
import { SheetsAuditService } from './sheets-audit.service';

@Component({
  selector: 'app-sheets-audit',
  imports: [PhaseSheetList, TypeSheetList, AuditSheetsForPhaseAndType],
  templateUrl: './sheets-audit.html',
  styleUrl: './sheets-audit.scss',
  providers: [TreeDragDropService, SheetsAuditService],
})
export class SheetsAudit {
  public readonly phaseId = input<number | undefined, unknown>(undefined, {
    transform: (v: unknown): number | undefined =>
      v != null ? numberAttribute(v) : undefined,
  });
  public readonly typeId = input<number | undefined, unknown>(undefined, {
    transform: (v: unknown): number | undefined =>
      v != null ? numberAttribute(v) : undefined,
  });

  public readonly activePhase = computed(() =>
    this._sheetService.activePhase(),
  );
  public readonly activeType = computed(() => this._sheetService.activeType());

  private readonly _sheetService = inject(SheetsAuditService);
  private readonly _phasesService = inject(PhasesService);
  private readonly _typesService = inject(TypesService);

  constructor() {
    effect(() => {
      const phaseId = this.phaseId();
      const phases = this._phasesService.specSheetPhases();
      if (!phaseId) {
        return;
      }

      const phase = SpecSheetPhaseHelpers.getPhaseByIdFromArray(
        phases,
        phaseId,
      );
      this._sheetService.activePhase.set(phase);
    });

    effect(() => {
      const typeId = this.typeId();
      const types = this._typesService.specSheetTypes();
      if (!typeId) {
        return;
      }

      const type = types.find((t) => t.id === typeId);
      this._sheetService.activeType.set(type);
    });
  }
}
