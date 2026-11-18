import { computed, linkedSignal } from '@angular/core';
import { UpdateHelpers } from '../../../../core/helpers/update-helpers';
import { WidgetType } from '../../../../core/models/domain/types/widget-type';
import { PFKNominalWidgetValue } from '../../../../core/models/domain/values/widget-values/pfk-nominal-widget-value';
import { SpecSheetOp } from '../../../../core/models/operations/spec-sheet-op';
import { ValidatorLevel } from '../../../../core/types/configuration/validator-level.enum';
import { PUSpecSheetTableGroup } from '../pu-spec-sheet-table-group';
import { SpecSheetAdvisory } from '../spec-sheet-advisory';
import { WidgetPropertyBase } from './widget-base';
import { SheetWidget } from '../../../../core/models/domain/sheets/sheet';

export class PFKNomWidget extends WidgetPropertyBase<
  SheetWidget,
  PUSpecSheetTableGroup,
  PFKNominalWidgetValue
> {
  public override readonly isValid = computed(() => this.value() !== undefined);
  public override readonly hasChanges = computed(
    () => this.value() !== this.rawWidgetValue()?.automaticNominalCalculation,
  );
  public override readonly value = linkedSignal(() => {
    const widgetValue = this.rawWidgetValue();
    if (!widgetValue) {
      return true;
    }

    return widgetValue.automaticNominalCalculation ?? true;
  });
  public override readonly widgetType = computed<WidgetType | undefined>(() => {
    const widget = this.widget;
    if (!widget) {
      return undefined;
    }

    const tableGroupTemplateWidget = this._sheetService
      .tableGroupTemplateWidgets()
      .find((tg) => tg.id === widget.tableGroupTemplateWidgetId);
    if (!tableGroupTemplateWidget) {
      return undefined;
    }

    return tableGroupTemplateWidget.widgetType;
  });
  public override readonly advisories = computed<SpecSheetAdvisory[]>(() => {
    const advisories: SpecSheetAdvisory[] = [];

    if (!this.isValid()) {
      if (this.value() === undefined) {
        advisories.push({
          level: ValidatorLevel.Error,
          message: 'PFK Nominal widget must have a value.',
          referenceId: '',
        });
      }
    }

    return advisories;
  });

  constructor(
    widget: SheetWidget,
    parent: PUSpecSheetTableGroup,
  ) {
    super(widget, parent);
  }

  public override toWidget(): SheetWidget {
    return {
      ...this.widget,
      value: {
        $type: 'pfkNominal',
        automaticNominalCalculation: this.value(),
      } as PFKNominalWidgetValue,
    };
  }

  public override *compileOperations(): Generator<SpecSheetOp> {
    if (this.isNew) {
      yield {
        $type: 'AddSpecSheetTableGroupWidgetOp',
        specSheetTableGroupId: this._parent.id,
        tableGroupTemplateWidgetId: this.widget.tableGroupTemplateWidgetId!,
        value: {
          $type: 'pfkNominal',
          automaticNominalCalculation: this.value(),
        } as PFKNominalWidgetValue,
      };
    } else if (this.hasChanges()) {
      yield {
        $type: 'UpdateSpecSheetTableGroupWidgetOp',
        id: this.widget.id,
        widgetValue: UpdateHelpers.compareToPropertyChanged(
          this.rawWidgetValue(),
          {
            $type: 'pfkNominal',
            automaticNominalCalculation: this.value(),
          } as PFKNominalWidgetValue,
        ),
      };
    }
  }

  private get isNew(): boolean {
    return this.widget.id <= 0;
  }
}
