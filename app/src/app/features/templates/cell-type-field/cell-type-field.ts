import { Component, computed, inject, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectChangeEvent, SelectModule } from 'primeng/select';
import { CellTypesStore } from '../cell-types.store';
import { CellKindInfo } from '../models/cell-kind-info.model';
import { CellKinds } from '../models/cell-kinds';
import { PanelNavigator } from '../panel-navigator';

/** A cell's type: a picker of every cell type, a button that opens the chosen one, and what its kind does. */
@Component({
  selector: 'app-cell-type-field',
  imports: [ButtonModule, FormsModule, SelectModule],
  templateUrl: './cell-type-field.html',
  styleUrl: './cell-type-field.scss',
})
export class CellTypeField {
  public readonly cellTypeId = input.required<number>();
  public readonly disabled = input(false);

  public readonly changed = output<number>();

  public readonly kind = computed<CellKindInfo>(() =>
    CellKinds.cellKindInfo(this.cellTypeStore.cellType(this.cellTypeId())?.kind ?? 'Text'),
  );

  public readonly typeOptions = computed(() =>
    this.cellTypeStore.cellTypes().map((cellType) => ({
      label: `${cellType.name} (${CellKinds.cellKindInfo(cellType.kind).label})`,
      value: cellType.id,
      icon: CellKinds.cellKindInfo(cellType.kind).icon,
    })),
  );

  private readonly cellTypeStore = inject(CellTypesStore);
  private readonly navigator = inject(PanelNavigator);

  public setType(event: SelectChangeEvent): void {
    this.changed.emit(event.value as number);
  }

  public openCellType(): void {
    const cellType = this.cellTypeStore.cellType(this.cellTypeId());
    if (cellType) {
      this.navigator.open({ kind: 'cellType', id: cellType.id });
    }
  }
}
