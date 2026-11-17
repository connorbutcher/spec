import { WidgetTypeKey } from '../../../core/models/domain/enums/widget-type-key';
import { SpecSheetTableGroupWidget } from '../../../core/models/domain/spec-sheet/spec-sheet-table-group-widget';
import { SpecSheetTableWidget } from '../../../core/models/domain/spec-sheet/spec-sheet-table-widget';
import { PUSpecSheetTableGroup } from '../models/pu-spec-sheet-table-group';
import { WidgetTypeRegistry } from '../models/services/widget-type-registry';
import { PUSpecSheetTableBase } from '../models/tables/pu-spec-sheet-table-base';
import { WidgetPropertyBase } from '../models/widgets/widget-base';

export class SpecSheetWidgetHelpers {
  public static mapTableWidgetToClass(
    tableWidget: SpecSheetTableWidget,
    widgetType: WidgetTypeKey,
    parent: PUSpecSheetTableBase,
  ): WidgetPropertyBase<SpecSheetTableWidget> {
    return WidgetTypeRegistry.createTableWidget(
      widgetType,
      tableWidget,
      parent,
    );
  }

  public static mapTableGroupWidgetToClass(
    groupWidget: SpecSheetTableGroupWidget,
    widgetType: WidgetTypeKey,
    parent: PUSpecSheetTableGroup,
  ): WidgetPropertyBase<SpecSheetTableGroupWidget> {
    return WidgetTypeRegistry.createGroupWidget(
      widgetType,
      groupWidget,
      parent,
    );
  }
}
