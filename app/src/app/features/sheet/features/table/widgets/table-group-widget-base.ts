import {
  Component,
  inject,
  Injector,
  input,
  model,
  Signal,
} from '@angular/core';
import { Destroyable } from '../../../../common/destructable/destructable';
import { PUSpecSheetTableGroup } from '../../../models/pu-spec-sheet-table-group';
import { PUSpecSheetTableBase } from '../../../models/tables/pu-spec-sheet-table-base';
import { WidgetPropertyBase } from '../../../models/widgets/widget-base';
import { ActiveSheetService } from '../../../services/active-sheet.service';
import { SheetWidget } from '../../../../../core/models/domain/sheets/sheet-widget';

@Component({
  selector: 'app-table-group-widget-base',
  template: '',
  standalone: true,
})
export class TableGroupWidgetBase<
  V,
  T extends WidgetPropertyBase<SheetWidget>,
> extends Destroyable {
  public readonly widget = input.required<T>();
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly group = input.required<PUSpecSheetTableGroup>();
  public readonly referenceId = input.required<string>();

  public readonly value = model<V>();

  protected readonly _injector = inject(Injector);
  protected readonly _sheetService = inject(ActiveSheetService);

  public get canEdit(): Signal<boolean> {
    return this._sheetService.canEdit;
  }
}
