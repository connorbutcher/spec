import { TableTemplate } from '../../../../../core/models/domain/template/table-template';
import { LinkedTableProperties } from '../components/linked-table-properties/linked-table-properties';

export class TablePropertiesComponentHelper {
  private static readonly _cellTypeMap = new Map<string, any>([
    ['linked', LinkedTableProperties],
  ]);

  public static getTableTypeComponent(tableTemplate: TableTemplate): any {
    return this._cellTypeMap.get(tableTemplate.defaultConfiguration?.$type);
  }
}
