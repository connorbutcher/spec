import { Component, inject, Injector, input, model } from '@angular/core';
import { Destroyable } from '../../../../common/destructable/destructable';
import { WidgetPropertyBase } from '../../../models/widgets/widget-base';
import { SheetWidget } from '../../../../../core/models/domain/sheets/sheet';

@Component({
  selector: 'app-table-widget-base',
  template: '',
  standalone: true,
})
export class TableWidgetBase<
  V,
  T extends WidgetPropertyBase<SheetWidget>,
> extends Destroyable {
  public readonly widget = input.required<T>();
  public readonly referenceId = input.required<string>();

  public readonly value = model<V>();

  protected readonly _injector = inject(Injector);
}
