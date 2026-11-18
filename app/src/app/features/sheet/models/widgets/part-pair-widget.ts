import { computed, linkedSignal } from '@angular/core';
import { UpdateHelpers } from '../../../../core/helpers/update-helpers';
import { WidgetType } from '../../../../core/models/domain/types/widget-type';
import { PartPairWidgetValue } from '../../../../core/models/domain/values/widget-values/part-pair-widget-value';
import { SpecSheetOp } from '../../../../core/models/operations/spec-sheet-op';
import { ValidatorLevel } from '../../../../core/types/configuration/validator-level.enum';
import { SpecSheetAdvisory } from '../spec-sheet-advisory';
import { PUSpecSheetTableBase } from '../tables/pu-spec-sheet-table-base';
import { WidgetPropertyBase } from './widget-base';
import { SheetWidget } from '../../../../core/models/domain/sheets/sheet-widget';

export class PartPairWidget extends WidgetPropertyBase<
  SheetWidget,
  PUSpecSheetTableBase,
  PartPairWidgetValue
> {
  public override readonly isValid = computed(
    () => this.value() !== undefined && this.valueBottom() !== undefined,
  );
  public override readonly hasChanges = computed(
    () =>
      this.value() !== this.rawWidgetValue()?.topValue ||
      this.valueBottom() !== this.rawWidgetValue()?.bottomValue,
  );
  public override readonly value = linkedSignal(() => {
    const widgetValue = this.rawWidgetValue();
    if (!widgetValue) {
      return undefined;
    }

    return widgetValue.topValue;
  });
  public readonly valueBottom = linkedSignal(() => {
    const widgetValue = this.rawWidgetValue();
    if (!widgetValue) {
      return undefined;
    }

    return widgetValue.bottomValue;
  });
  public override readonly widgetType = computed<WidgetType | undefined>(() => {
    const widget = this.widget;
    if (!widget) {
      return undefined;
    }

    const tableTemplateWidget = this._sheetService
      .tableTemplateWidgets()
      .find((tg) => tg.id === widget.tableTemplateWidgetId);
    if (!tableTemplateWidget) {
      return undefined;
    }

    return tableTemplateWidget.widgetType;
  });
  public override readonly advisories = computed<SpecSheetAdvisory[]>(() => {
    const advisories: SpecSheetAdvisory[] = [];

    if (!this.isValid()) {
      if (this.value() === undefined || this.valueBottom() === undefined) {
        advisories.push({
          level: ValidatorLevel.Error,
          message: 'Part Pair identifier values are required. e.g. RH/LH',
          referenceId: '',
        });
      }
    }

    return advisories;
  });

  constructor(widget: SheetWidget, parent: PUSpecSheetTableBase) {
    super(widget, parent);
  }

  public override toWidget(): SheetWidget {
    return {
      ...this.widget,
      value: {
        $type: 'partPair',
        topValue: this.value(),
        bottomValue: this.valueBottom(),
      } as PartPairWidgetValue,
    };
  }

  public override *compileOperations(): Generator<SpecSheetOp> {
    if (this.isNew) {
      yield {
        $type: 'AddSpecSheetTableWidgetOp',
        specSheetTableId: this._parent.id,
        tableTemplateWidgetId: this.widget.tableTemplateWidgetId!,
        value: {
          $type: 'partPair',
          topValue: this.value(),
          bottomValue: this.valueBottom(),
        } as PartPairWidgetValue,
      };
    } else if (this.hasChanges()) {
      yield {
        $type: 'UpdateSpecSheetTableWidgetOp',
        id: this.widget.id,
        widgetValue: UpdateHelpers.compareToPropertyChanged(
          this.rawWidgetValue(),
          {
            $type: 'partPair',
            topValue: this.value(),
            bottomValue: this.valueBottom(),
          } as PartPairWidgetValue,
        ),
      };
    }
  }

  private get isNew(): boolean {
    return this.widget.id <= 0;
  }
}
