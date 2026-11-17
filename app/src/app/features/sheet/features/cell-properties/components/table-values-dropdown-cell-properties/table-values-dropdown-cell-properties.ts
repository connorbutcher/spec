import {
  Component,
  computed,
  effect,
  inject,
  input,
  model,
  Signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SelectItem, TreeNode } from 'primeng/api';
import { Select } from 'primeng/select';
import { SelectButton } from 'primeng/selectbutton';
import { TreeSelect } from 'primeng/treeselect';
import { SpecSheetPhaseHelpers } from '../../../../../../core/helpers/spec-sheet-phase-helpers';
import { SpecSheetPhase } from '../../../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../../../core/models/domain/types/spec-sheet-type';
import { PhasesService } from '../../../../../../core/services/phases.service';
import { TypesService } from '../../../../../../core/services/types.service';
import { ElementLabel } from '../../../../../common/element-label/element-label';
import { SheetsService } from '../../../../../sheets/services/sheets.service';
import { ActiveSheetService } from '../../../../services/active-sheet.service';
import { TableValuesDropdownCellProperty } from '../../models/table-values-dropdown-cell-property';

@Component({
  selector: 'app-table-values-dropdown-cell-properties',
  imports: [ElementLabel, Select, FormsModule, TreeSelect, SelectButton],
  templateUrl: './table-values-dropdown-cell-properties.html',
  styleUrl: './table-values-dropdown-cell-properties.scss',
})
export class TableValuesDropdownCellProperties {
  public readonly cellProperty =
    input.required<TableValuesDropdownCellProperty>();

  public readonly specSheetPhase = model<TreeNode<SpecSheetPhase> | undefined>(
    undefined,
  );
  public readonly tableOptions = computed(() =>
    this._activeSheetService.specSheet().tables(),
  );
  public readonly rowOptions = computed(() => {
    const table = this.cellProperty().table();
    if (!table) {
      return [];
    }

    const rowOptions: SelectItem[] = [];
    const rows = table
      .groups()
      .flatMap((g) =>
        g.rows().filter((r) => r.tableRowTemplate()?.canBeReordered),
      );
    for (const row of rows) {
      rowOptions.push({
        label: row.cells()[0].value() || `Row ${row.rowNumber()}`,
        value: row,
      });
    }

    return rowOptions;
  });
  public readonly cellOptions = computed(() => {
    const row = this.cellProperty().row();
    if (!row) {
      return [];
    }

    const childGroups = row.parent.childGroups();
    for (const childGroup of childGroups) {
      const matchingRow = childGroup
        .rows()
        .find((r) => r.rowNumber() === row.rowNumber());

      if (!matchingRow) {
        continue;
      }

      return matchingRow.cells().map((cell) => ({
        label: cell.tableCellType().title,
        value: cell.tableCellType(),
      }));
    }

    return [];
  });
  public readonly phaseOptions = computed<TreeNode<SpecSheetPhase>[]>(() => {
    const phases = this._phasesService.specSheetPhases();

    const nodes: TreeNode[] = [];
    for (const phase of phases) {
      nodes.push({
        key: `${phase.id}`,
        label: phase.phase,
        data: phase,
        children: phase.phases.map((subPhase) => ({
          key: `${subPhase.id}`,
          label: subPhase.phase,
          data: subPhase,
        })),
      });
    }

    return nodes;
  });
  public readonly lockToLatestPublishedOptions = [
    { label: 'Yes', value: true },
    { label: 'No', value: false },
  ];

  private readonly _activeSheetService = inject(ActiveSheetService);
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
      const type = this._typesService
        .specSheetTypes()
        .find((sst) => sst.id === this._sheetsService.selectedType()!.id);
      if (type) {
        this.cellProperty().specSheetType.set(type);
      }
    });

    effect(() => {
      const phase = SpecSheetPhaseHelpers.getPhaseByIdFromArray(
        this._phasesService.specSheetPhases(),
        this._phasesService.selectedPhase()!.id,
      );
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

  public get specSheetTypes(): Signal<SpecSheetType[]> {
    return this._typesService.specSheetTypes;
  }
}
