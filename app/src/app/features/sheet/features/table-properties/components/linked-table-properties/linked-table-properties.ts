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
import { LinkedTableProperty } from '../../models/linked-table-property';

@Component({
  selector: 'app-linked-table-properties',
  imports: [ElementLabel, Select, FormsModule, TreeSelect, SelectButton],
  templateUrl: './linked-table-properties.html',
  styleUrl: './linked-table-properties.scss',
})
export class LinkedTableProperties {
  public readonly tableProperty = input.required<LinkedTableProperty>();
  public readonly tableOptions = computed(() =>
    this.tableProperty().linkedSpecSheet()?.tables(),
  );
  public readonly phaseOptions = computed<TreeNode<SpecSheetPhase>[]>(() => {
    const phases = this._phasesService.specSheetPhases();
    const typeSelected = this.tableProperty().specSheetType();

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

  constructor() {
    effect(() => {
      const specSheetPhaseNode = this.specSheetPhase();
      if (specSheetPhaseNode) {
        this.tableProperty().specSheetPhase.set(specSheetPhaseNode.data);
      }
    });

    effect(() => {
      const phase = this.tableProperty().specSheetPhase();
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

  private readonly _phasesService = inject(PhasesService);
  private readonly _typesService = inject(TypesService);

  public get specSheetTypes() {
    return this._typesService.specSheetTypes;
  }
}
