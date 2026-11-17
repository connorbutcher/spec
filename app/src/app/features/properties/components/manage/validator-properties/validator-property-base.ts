import { Component, input, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Destroyable } from '../../../../common/destructable/destructable';

@Component({
  selector: 'app-validator-property-base',
  template: '',
})
export class ValidatorPropertyBase<TValue>
  extends Destroyable
  implements OnInit
{
  public readonly control = input.required<FormControl<TValue>>();

  ngOnInit(): void {}
}
