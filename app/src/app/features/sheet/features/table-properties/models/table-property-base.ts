import { inject } from '@angular/core';
import { TableProperties } from '../../../../../core/models/domain/properties/table/table-properties';
import { TableTemplate } from '../../../../../core/models/domain/template/table-template';
import { ConfigurablePropertyBase } from '../../../models/services/configurable-property-base';
import { SheetContext } from '../../../services/sheet-context';

export abstract class TablePropertyBase<T> extends ConfigurablePropertyBase {
  protected readonly _sheetService = inject(SheetContext);

  constructor(
    public readonly tableTemplate: TableTemplate,
    public readonly properties: T,
  ) {
    super();
  }

  public abstract set(value: T): void;
  public abstract toTemplate(): TableTemplate;
  public abstract toConfiguration(): TableProperties;
}
