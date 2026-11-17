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
import { MinMaxLengthValidatorProperty } from '../../../../../../core/models/domain/configuration/validator-properties/min-max-length-validator-property';
import { ElementLabel } from '../../../../../common/element-label/element-label';
import { ValidatorPropertyBase } from '../validator-property-base';

@Component({
  selector: 'app-min-max-length-validator-properties',
  imports: [InputNumber, FormsModule, ReactiveFormsModule, ElementLabel],
  templateUrl: './min-max-length-validator-properties.html',
  styleUrls: [
    './min-max-length-validator-properties.scss',
    '../validator-properties.scss',
  ],
})
export class MinMaxLengthValidatorProperties extends ValidatorPropertyBase<MinMaxLengthValidatorProperty> {
  public readonly lengthCtrl = new FormControl<number>(undefined!, {
    validators: [Validators.required],
    nonNullable: true,
  });

  constructor() {
    super();

    combineLatest([toObservable(this.control), this.lengthCtrl.valueChanges])
      .pipe(takeUntilDestroyed())
      .subscribe(([control, value]) => {
        if (!control) {
          return;
        }

        control.setValue({
          $type: 'minMaxLength',
          number: value,
        });
      });
  }

  override ngOnInit(): void {
    const defaultValue = this.control().value;
    if (defaultValue) {
      this.lengthCtrl.setValue(defaultValue.number, { emitEvent: false });
    }

    if (this.control().disabled) {
      this.lengthCtrl.disable();
    }
  }
}
