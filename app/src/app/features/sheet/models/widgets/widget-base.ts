import { inject, signal, Signal } from '@angular/core';
import { WidgetType } from '../../../../core/models/domain/types/widget-type';
import { WidgetValue } from '../../../../core/models/domain/values/widget-values/widget-value';
import { SpecSheetOp } from '../../../../core/models/operations/spec-sheet-op';
import { SheetContext } from '../../services/sheet-context';
import { PUSpecSheetTableGroup } from '../pu-spec-sheet-table-group';
import { SpecSheetAdvisory } from '../spec-sheet-advisory';
import { PUSpecSheetTableBase } from '../tables/pu-spec-sheet-table-base';
import { SheetWidget } from '../../../../core/models/domain/sheets/sheet-widget';

export abstract class WidgetPropertyBase<
  T extends SheetWidget,
  C = PUSpecSheetTableBase | PUSpecSheetTableGroup,
  V extends WidgetValue = WidgetValue,
> {
  public abstract readonly isValid: Signal<boolean>;
  public abstract readonly hasChanges: Signal<boolean>;
  public abstract readonly widgetType: Signal<WidgetType | undefined>;
  public abstract readonly value: Signal<any>;
  public abstract readonly advisories: Signal<SpecSheetAdvisory[]>;
  public readonly rawWidgetValue: Signal<V | undefined>;

  protected readonly _sheetService = inject(SheetContext);

  constructor(
    public readonly widget: T,
    protected readonly _parent: C,
  ) {
    this.rawWidgetValue = signal(widget.value as V | undefined);
  }

  public abstract toWidget(): T;
  public abstract compileOperations(): Generator<SpecSheetOp>;
}
