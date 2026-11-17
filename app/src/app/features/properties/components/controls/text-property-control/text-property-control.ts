import { Component, OnInit } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { InputText } from 'primeng/inputtext';
import { combineLatest, take } from 'rxjs';
import { StringPropertyValue } from '../../../../../core/models/domain/spec-sheet/properties/values/string-property-value';
import { PropertyValueType } from '../../../../../core/types/properties/property-value-type.enum';
import { ElementLabel } from '../../../../common/element-label/element-label';
import { PropertyControlBase } from '../property-control-base';

@Component({
  selector: 'app-text-property-control',
  imports: [ReactiveFormsModule, FormsModule, ElementLabel, InputText],
  templateUrl: './text-property-control.html',
  styleUrl: './text-property-control.scss',
})
export class TextPropertyControl
  extends PropertyControlBase<string, StringPropertyValue>
  implements OnInit
{
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

        control.setValue({ $type: PropertyValueType.String, value: value });
      });
  }

  override ngOnInit(): void {
    super.ngOnInit();
  }
}
