import {
  Component,
  computed,
  effect,
  inject,
  input,
  model,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TreeNode } from 'primeng/api';
import { Select } from 'primeng/select';
import { SelectButton } from 'primeng/selectbutton';
import { TreeSelect } from 'primeng/treeselect';
import { SpecSheetPhase } from '../../../../../../core/models/domain/types/spec-sheet-phase';
import { PhasesService } from '../../../../../../core/services/phases.service';
import { TypesService } from '../../../../../../core/services/types.service';
import { ElementLabel } from '../../../../../common/element-label/element-label';
import { SheetsService } from '../../../../../sheets/services/sheets.service';
import { LinkedSpecSheetValueDropdownPhaseTypeCellProperty } from '../../models/linked-spec-sheet-value-dropdown-phase-type-cell-property';

@Component({
  selector: 'app-linked-spec-sheet-dropdown-cell-properties',
  imports: [ElementLabel, Select, FormsModule, TreeSelect, SelectButton],
  templateUrl: './linked-spec-sheet-dropdown-cell-properties.html',
  styleUrl: './linked-spec-sheet-dropdown-cell-properties.scss',
})
export class LinkedSpecSheetDropdownCellProperties {
  public readonly cellProperty =
    input.required<LinkedSpecSheetValueDropdownPhaseTypeCellProperty>();
  public readonly tableOptions = computed(() => {
    return this.cellProperty().linkedSpecSheet()?.tables();
  });
  public readonly specSheetTypes = computed(() =>
    this._typesService
      .specSheetTypes()
      .filter((t) => t.id !== this._sheetsService.selectedType()!.id),
  );
  public readonly phaseOptions = computed<TreeNode<SpecSheetPhase>[]>(() => {
    const phases = this._phasesService.specSheetPhases();
    const typeSelected = this.cellProperty().specSheetType();

    const nodes: TreeNode[] = [];
    for (const phase of phases) {
      const phaseHasType = phase.types.some((t) => t.id === typeSelected?.id);
      const subPhaseHasType = phase.phases.some((subPhase) =>
        subPhase.types.some((t) => t.id === typeSelected?.id),
      );

      nodes.push({
        key: `${phase.id}`,
        label: phase.phase,
        data: phase,
        selectable: phaseHasType,
        styleClass: phaseHasType || subPhaseHasType ? '' : 'p-disabled',
        children: phase.phases.map((subPhase) => {
          const subPhaseHasType = subPhase.types.some(
            (t) => t.id === typeSelected?.id,
          );

          return {
            key: `${subPhase.id}`,
            label: subPhase.phase,
            data: subPhase,
            selectable: subPhaseHasType,
            styleClass: subPhaseHasType ? '' : 'p-disabled',
          };
        }),
      });
    }

    return nodes;
  });
  public readonly specSheetPhase = model<TreeNode<SpecSheetPhase> | undefined>(
    undefined,
  );
  public readonly lockToLatestPublishedOptions = [
    { label: 'Yes', value: true },
    { label: 'No', value: false },
  ];

  private readonly _sheetsService = inject(SheetsService);
  private readonly _phasesService = inject(PhasesService);
  private readonly _typesService = inject(TypesService);

  constructor() {
    effect(() => {
      const specSheetPhaseNode = this.specSheetPhase();
      if (specSheetPhaseNode) {
        this.cellProperty().specSheetPhase.set(specSheetPhaseNode.data);
      }
    });

    effect(() => {
      const phase = this.cellProperty().specSheetPhase();
      const phaseOptions = this.phaseOptions();
      if (!phase || !phaseOptions) {
        return;
      }

      // Search the tree for the matching phase
      let foundNode: TreeNode<SpecSheetPhase> | undefined;
      for (const node of phaseOptions) {
        if (node.data?.id === phase.id) {
          foundNode = node;
          break;
        }

        if (node.children) {
          for (const child of node.children) {
            if (child.data?.id === phase.id) {
              foundNode = child;
              break;
            }
          }
        }

        if (foundNode) {
          break;
        }
      }

      if (foundNode) {
        this.specSheetPhase.set(foundNode);
      }
    });
  }
}
