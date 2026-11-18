import { WidgetTypeKey } from '../../../../core/models/domain/enums/widget-type-key';
import { PUSpecSheetTableGroup } from '../pu-spec-sheet-table-group';
import { PUSpecSheetTableBase } from '../tables/pu-spec-sheet-table-base';
import { PartPairWidget } from '../widgets/part-pair-widget';
import { PFKNomWidget } from '../widgets/pfk-nominal-widget';
import { WidgetPropertyBase } from '../widgets/widget-base';
import { SheetWidget } from '../../../../core/models/domain/sheets/sheet';

export type TableWidgetFactory = (
  widget: SheetWidget,
  parent: PUSpecSheetTableBase,
) => WidgetPropertyBase<SheetWidget>;

export type GroupWidgetFactory = (
  widget: SheetWidget,
  parent: PUSpecSheetTableGroup,
) => WidgetPropertyBase<SheetWidget>;

export class WidgetTypeRegistry {
  private static readonly _tableWidgets = new Map<
    WidgetTypeKey,
    TableWidgetFactory
  >();
  private static readonly _groupWidgets = new Map<
    WidgetTypeKey,
    GroupWidgetFactory
  >();

  static {
    WidgetTypeRegistry.registerDefaults();
  }

  public static registerTableWidget(
    type: WidgetTypeKey,
    factory: TableWidgetFactory,
  ): void {
    this._tableWidgets.set(type, factory);
  }

  public static registerGroupWidget(
    type: WidgetTypeKey,
    factory: GroupWidgetFactory,
  ): void {
    this._groupWidgets.set(type, factory);
  }

  public static createTableWidget(
    type: WidgetTypeKey,
    widget: SheetWidget,
    parent: PUSpecSheetTableBase,
  ): WidgetPropertyBase<SheetWidget> {
    const factory = this._tableWidgets.get(type);
    if (!factory) {
      throw new Error(`Unsupported table widget type: ${type}`);
    }

    return factory(widget, parent);
  }

  public static createGroupWidget(
    type: WidgetTypeKey,
    widget: SheetWidget,
    parent: PUSpecSheetTableGroup,
  ): WidgetPropertyBase<SheetWidget> {
    const factory = this._groupWidgets.get(type);
    if (!factory) {
      throw new Error(`Unsupported group widget type: ${type}`);
    }

    return factory(widget, parent);
  }

  private static registerDefaults(): void {
    this.registerTableWidget(
      WidgetTypeKey.PartPairIdentifier,
      (widget, parent) =>
        new PartPairWidget(widget, parent as PUSpecSheetTableBase),
    );
    this.registerGroupWidget(
      WidgetTypeKey.PFKNominal,
      (widget, parent) =>
        new PFKNomWidget(widget, parent as PUSpecSheetTableGroup),
    );
  }
}
