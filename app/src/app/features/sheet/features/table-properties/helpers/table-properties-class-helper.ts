import { TableProperties } from '../../../../../core/models/domain/properties/table/table-properties';
import { TableTemplate } from '../../../../../core/models/domain/template/table-template';
import { LinkedTableProperty } from '../models/linked-table-property';
import { TablePropertyBase } from '../models/table-property-base';

export class TablePropertiesClassHelpers {
  public static map(
    tableTemplate: TableTemplate,
    configurationOverride?: TableProperties,
  ): TablePropertyBase<TableProperties> {
    switch (tableTemplate?.defaultConfiguration?.$type) {
      case 'linked':
        return new LinkedTableProperty(
          tableTemplate,
          configurationOverride ?? tableTemplate.defaultConfiguration,
        );
      default:
        return undefined!;
    }
  }
}
