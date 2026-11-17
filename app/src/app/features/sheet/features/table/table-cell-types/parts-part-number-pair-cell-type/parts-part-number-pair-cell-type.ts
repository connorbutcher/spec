import { Component, computed, model, OnInit } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ButtonDirective } from 'primeng/button';
import { Tooltip } from 'primeng/tooltip';
import { filter, skip, takeUntil } from 'rxjs';
import { WidgetTypeKey } from '../../../../../../core/models/domain/enums/widget-type-key';
import { PUSpecSheetTableCellPair } from '../../../../models/cells/pu-spec-sheet-table-cell-pair';
import { PartPairWidget } from '../../../../models/widgets/part-pair-widget';
import { TextInput } from '../../inputs/text-input/text-input';
import { PartsPairWidget } from '../../widgets/parts-pair-widget/parts-pair-widget';
import { CellTypeBase } from '../cell-type-base';

@Component({
  selector: 'app-parts-part-number-pair-cell-type',
  imports: [FormsModule, ButtonDirective, Tooltip, PartsPairWidget, TextInput],
  templateUrl: './parts-part-number-pair-cell-type.html',
  styleUrl: './parts-part-number-pair-cell-type.scss',
  host: { class: 'cell-host' },
})
export class PartsPartNumberPairCellType
  extends CellTypeBase<string, PUSpecSheetTableCellPair>
  implements OnInit
{
  public readonly cellValueY = model<string | undefined>(undefined);

  public readonly addOptions = computed(() => {
    const table = this.table();
    const group = this.group();
    if (!table || !group) {
      return [];
    }

    const tableType = table.tableType();
    return tableType.tableGroupTemplates.filter(
      (tg) => tg.parentTableGroupTemplateId === group.tableGroupTemplate()?.id,
    );
  });
  public readonly partPairWidget = computed(() => {
    const table = this.table();
    if (!table) {
      return undefined;
    }

    return table
      .widgets()
      .find((w) => w.widgetType()?.key === WidgetTypeKey.PartPairIdentifier) as
      | PartPairWidget
      | undefined;
  });

  constructor() {
    super();

    toObservable(this.cellValue)
      .pipe(skip(1), takeUntilDestroyed())
      .subscribe((value) => {
        if (value?.trim()?.length === 0) {
          this.cell().value.set(undefined);
          return;
        }

        this.cell().value.set(value?.trim());
      });

    toObservable(this.cellValueY)
      .pipe(skip(1), takeUntilDestroyed())
      .subscribe((value) => {
        if (value?.trim()?.length === 0) {
          this.cell().valueY.set(undefined);
          return;
        }

        this.cell().valueY.set(value?.trim());
      });
  }

  ngOnInit(): void {
    toObservable(this.cell().value, { injector: this._injector })
      .pipe(
        takeUntil(this.destroy),
        filter((value) => value !== this.cellValue()),
      )
      .subscribe(() => {
        this.cellValue.set(this.cell().value() ?? undefined);
      });

    toObservable(this.cell().valueY, { injector: this._injector })
      .pipe(
        takeUntil(this.destroy),
        filter((value) => value !== this.cellValueY()),
      )
      .subscribe(() => {
        this.cellValueY.set(this.cell().valueY() ?? undefined);
      });
  }

  public deletePart(): void {
    const table = this.table();
    const group = this.group();
    const parentGroup = table
      .groups()
      .find((g) => g.id === group.parentGroupId());

    if (!table || !group || !parentGroup) {
      return;
    }

    parentGroup.removeGroup(group, false);
  }
}
