import { Component } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { filter, skip, takeUntil } from 'rxjs';
import { TextInput } from '../../inputs/text-input/text-input';
import { CellTypeBase } from '../cell-type-base';

@Component({
  selector: 'app-text-cell-type',
  imports: [FormsModule, TextInput],
  templateUrl: './text-cell-type.html',
  styleUrl: './text-cell-type.scss',
  host: { class: 'width-100' },
})
export class TextCellType extends CellTypeBase<string | undefined> {
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
