import { Component, effect } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { ButtonDirective } from 'primeng/button';
import { Tooltip } from 'primeng/tooltip';
import { filter, skip, takeUntil } from 'rxjs';
import { MathHelpers } from '../../../../../../core/helpers/math-helpers';
import { CellTypeReferenceKey } from '../../../../../../core/models/domain/enums/cell-type-reference-key';
import { PFKNomWidget } from '../../../../models/widgets/pfk-nominal-widget';
import { TableGroupWidgetBase } from '../table-group-widget-base';

@Component({
  selector: 'app-pfk-nominal-widget',
  imports: [ButtonDirective, Tooltip],
  templateUrl: './pfk-nominal-widget.html',
  styleUrl: './pfk-nominal-widget.scss',
  host: { class: 'cell-host' },
})
export class PFKNominalWidget extends TableGroupWidgetBase<
  boolean,
  PFKNomWidget
> {
  constructor() {
    super();
    toObservable(this.value)
      .pipe(skip(1), takeUntilDestroyed())
      .subscribe((value) => {
        this.widget().value.set(value!);
      });

    effect(() => {
      if (!this.widget().value()) {
        return;
      }

      for (const group of this.group().childGroups()) {
        for (const row of group.rows()) {
          const minCell = row
            .cells()
            .find((c) => c.tableCellType().key === CellTypeReferenceKey.Min);
          const maxCell = row
            .cells()
            .find((c) => c.tableCellType().key === CellTypeReferenceKey.Max);
          const nomCell = row
            .cells()
            .find((c) => c.tableCellType().key === CellTypeReferenceKey.Nom);

          if (!minCell || !maxCell || !nomCell) {
            continue;
          }

          if (this.widget().value()) {
            if (
              MathHelpers.isNan(minCell.value()) ||
              MathHelpers.isNan(maxCell.value())
            ) {
              continue;
            }

            nomCell.value.set(
              MathHelpers.round((minCell.value() + maxCell.value()) / 2, 4),
            );
          }
        }
      }
    });
  }

  ngOnInit(): void {
    toObservable(this.widget().value, { injector: this._injector })
      .pipe(
        takeUntil(this.destroy),
        filter((value) => value !== this.value()),
      )
      .subscribe(() => {
        this.value.set(this.widget().value());
      });
  }

  public toggleNominal(): void {
    this.widget().value.set(!this.widget().value());
    this.table().triggerRebuild();
  }
}
