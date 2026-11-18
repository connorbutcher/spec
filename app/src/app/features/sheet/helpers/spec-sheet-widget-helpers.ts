import { WidgetTypeKey } from '../../../core/models/domain/enums/widget-type-key';
import { PUSpecSheetTableGroup } from '../models/pu-spec-sheet-table-group';
import { WidgetTypeRegistry } from '../models/services/widget-type-registry';
import { PUSpecSheetTableBase } from '../models/tables/pu-spec-sheet-table-base';
import { WidgetPropertyBase } from '../models/widgets/widget-base';
import { SheetWidget } from '../../../core/models/domain/sheets/sheet';

export class SpecSheetWidgetHelpers {
  public static mapTableWidgetToClass(
    tableWidget: SheetWidget,
    widgetType: WidgetTypeKey,
    parent: PUSpecSheetTableBase,
  ): WidgetPropertyBase<SheetWidget> {
    return WidgetTypeRegistry.createTableWidget(
      widgetType,
      tableWidget,
      parent,
    );
  }

  public static mapTableGroupWidgetToClass(
    groupWidget: SheetWidget,
    widgetType: WidgetTypeKey,
    parent: PUSpecSheetTableGroup,
  ): WidgetPropertyBase<SheetWidget> {
    return WidgetTypeRegistry.createGroupWidget(
      widgetType,
      groupWidget,
      parent,
    );
  }
}
