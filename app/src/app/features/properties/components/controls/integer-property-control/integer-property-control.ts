import { Component } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule } from '@angular/forms';
import { toInteger } from 'lodash';
import { InputNumber } from 'primeng/inputnumber';
import { combineLatest, take } from 'rxjs';
import { IntPropertyValue } from '../../../../../core/models/domain/values/property-values/int-property-value';
import { PropertyValueType } from '../../../../../core/types/properties/property-value-type.enum';
import { ElementLabel } from '../../../../common/element-label/element-label';
import { PropertyControlBase } from '../property-control-base';

@Component({
  selector: 'app-integer-property-control',
  imports: [ReactiveFormsModule, ElementLabel, InputNumber],
  templateUrl: './integer-property-control.html',
  styleUrl: './integer-property-control.scss',
})
export class IntegerPropertyControl extends PropertyControlBase<
  number,
  IntPropertyValue
> {
  constructor() {
    super();

    toObservable(this.control)
      .pipe(take(1))
      .subscribe((value) => {
        if (value && value.value) {
          this.formControl.setValue(value.value.value!);
        }
      });

    combineLatest([toObservable(this.control), this.formControl.valueChanges])
      .pipe(takeUntilDestroyed())
      .subscribe(([control, value]) => {
        if (!control) {
          return;
        }

        const intValue = toInteger(value);
        control.setValue({ $type: PropertyValueType.Int, value: intValue });
      });
  }
}
