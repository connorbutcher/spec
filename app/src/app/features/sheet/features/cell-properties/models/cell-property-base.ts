import { CellProperties } from '../../../../../core/models/domain/properties/cell/cell-properties';
import { TableCellTemplate } from '../../../../../core/models/domain/template/table-cell-template';
import { TableCellType } from '../../../../../core/models/domain/template/table-cell-type';
import { ConfigurablePropertyBase } from '../../../models/services/configurable-property-base';

export abstract class CellPropertyBase<T> extends ConfigurablePropertyBase {
  constructor(
    public readonly tableCellType: TableCellType,
    public readonly properties: T,
    public readonly cellTemplate?: TableCellTemplate,
  ) {
    super();
  }

  public abstract set(value: T): void;
  public abstract toCellTemplate(): TableCellTemplate;
  public abstract toCellConfiguration(): CellProperties;
}
