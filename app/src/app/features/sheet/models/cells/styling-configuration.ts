import { CellConfiguration } from '../../../../core/models/domain/configuration/cell-configuration';
import { TableCellType } from '../../../../core/models/domain/template/table-cell-type';

export class StylingConfiguration {
  public readonly width: number | undefined;
  public readonly backgroundColor: string;
  public readonly fontWeight: number;
  public readonly fontSize: string;
  public readonly isSticky: boolean;

  constructor(
    public readonly tableCellType: TableCellType,
    public readonly configuration: CellConfiguration,
  ) {
    this.width = configuration.styling.width ?? tableCellType.styling.width;
    this.backgroundColor = configuration.styling.backgroundColor;
    this.fontWeight =
      configuration.styling.fontWeight ?? tableCellType.styling.fontWeight;
    this.fontSize =
      configuration.styling.fontSize ??
      tableCellType.styling.fontSize ??
      'inherit';
    this.isSticky =
      configuration.styling?.isSticky ?? tableCellType.styling.isSticky;
  }
}
