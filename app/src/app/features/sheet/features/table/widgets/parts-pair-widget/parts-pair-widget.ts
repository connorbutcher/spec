import { Component, model } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { filter, skip, takeUntil } from 'rxjs';
import { PartPairWidget } from '../../../../models/widgets/part-pair-widget';
import { TextInput } from '../../inputs/text-input/text-input';
import { TableWidgetBase } from '../table-widget-base';

@Component({
  selector: 'app-parts-pair-widget',
  imports: [FormsModule, TextInput],
  templateUrl: './parts-pair-widget.html',
  styleUrl: './parts-pair-widget.scss',
  host: { class: 'cell-host' },
})
export class PartsPairWidget extends TableWidgetBase<
  string | undefined,
  PartPairWidget
> {
  public readonly valueBottom = model<string | undefined>(undefined);

  constructor() {
    super();
    toObservable(this.value)
      .pipe(skip(1), takeUntilDestroyed())
      .subscribe((value) => {
        if (value?.trim()?.length === 0) {
          this.widget().value.set(undefined);
          return;
        }

        this.widget().value.set(value?.trim());
      });

    toObservable(this.valueBottom)
      .pipe(skip(1), takeUntilDestroyed())
      .subscribe((value) => {
        if (value?.trim()?.length === 0) {
          this.widget().valueBottom.set(undefined);
          return;
        }

        this.widget().valueBottom.set(value?.trim());
      });
  }

  ngOnInit(): void {
    toObservable(this.widget().value, { injector: this._injector })
      .pipe(
        takeUntil(this.destroy),
        filter((value) => value !== this.value()),
      )
      .subscribe(() => {
        this.value.set(this.widget().value() ?? undefined);
      });

    toObservable(this.widget().valueBottom, { injector: this._injector })
      .pipe(
        takeUntil(this.destroy),
        filter((value) => value !== this.valueBottom()),
      )
      .subscribe(() => {
        this.valueBottom.set(this.widget().valueBottom() ?? undefined);
      });
  }
}
