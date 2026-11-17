import { Component } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { InputNumber } from 'primeng/inputnumber';
import { combineLatest } from 'rxjs';
import { MinMaxNumberValidatorProperty } from '../../../../../../core/models/domain/configuration/validator-properties/min-max-number-validator-property';
import { ElementLabel } from '../../../../../common/element-label/element-label';
import { ValidatorPropertyBase } from '../validator-property-base';

@Component({
  selector: 'app-min-max-number-validator-properties',
  imports: [InputNumber, FormsModule, ReactiveFormsModule, ElementLabel],
  templateUrl: './min-max-number-validator-properties.html',
  styleUrls: [
    './min-max-number-validator-properties.scss',
    '../validator-properties.scss',
  ],
})
export class MinMaxNumberValidatorProperties extends ValidatorPropertyBase<MinMaxNumberValidatorProperty> {
  public readonly numberCtrl = new FormControl<number>(undefined!, {
    validators: [Validators.required],
    nonNullable: true,
  });

  constructor() {
    super();

    combineLatest([toObservable(this.control), this.numberCtrl.valueChanges])
      .pipe(takeUntilDestroyed())
      .subscribe(([control, value]) => {
        if (!control) {
          return;
        }

        control.setValue({
          $type: 'minMaxNumber',
          number: value,
        });
      });
  }

  override ngOnInit(): void {
    const defaultValue = this.control().value;
    if (defaultValue) {
      this.numberCtrl.setValue(defaultValue.number, { emitEvent: false });
    }

    if (this.control().disabled) {
      this.numberCtrl.disable();
    }
  }
}
