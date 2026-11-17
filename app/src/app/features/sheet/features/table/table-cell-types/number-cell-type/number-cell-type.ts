import { A11yModule } from '@angular/cdk/a11y';
import { DecimalPipe } from '@angular/common';
import { Component } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { filter, skip, takeUntil } from 'rxjs';
import { FocusDirective } from '../../../../../../core/directives/focus.directive';
import { CellTypeBase } from '../cell-type-base';

@Component({
  selector: 'app-number-cell-type',
  imports: [FormsModule, A11yModule, FocusDirective, DecimalPipe],
  templateUrl: './number-cell-type.html',
  styleUrl: './number-cell-type.scss',
  host: { class: 'cell-host' },
})
export class NumberCellType extends CellTypeBase<number> {
  constructor() {
    super();

    toObservable(this.cellValue)
      .pipe(skip(1), takeUntilDestroyed())
      .subscribe((value) => {
        this.cell().value.set(value);
      });
  }

  ngOnInit(): void {
    toObservable(this.cell().value, { injector: this._injector })
      .pipe(
        takeUntil(this.destroy),
        filter((value) => value !== this.cellValue()),
      )
      .subscribe(() => {
        this.cellValue.set(this.cell().value());
      });
  }
}
