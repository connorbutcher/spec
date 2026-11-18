import { Signal } from '@angular/core';
import { TableGroupTemplate } from '../../../core/models/domain/template/table-group-template';
import { TableGroupTemplateWidget } from '../../../core/models/domain/template/table-group-template-widget';
import { TableRowTemplate } from '../../../core/models/domain/template/table-row-template';
import { TableTemplateWidget } from '../../../core/models/domain/template/table-template-widget';
import { PUSpecSheet } from '../models/pu-spec-sheet';
import { SheetExternalReference } from '../../../core/models/domain/sheets/sheet-external-reference';

export abstract class SheetContext {
  public abstract readonly specSheet: Signal<PUSpecSheet>;
  public abstract readonly externalReferences: Signal<SheetExternalReference[]>;
  public abstract readonly tableGroupTemplates: Signal<TableGroupTemplate[]>;
  public abstract readonly tableRowTemplates: Signal<TableRowTemplate[]>;
  public abstract readonly tableTemplateWidgets: Signal<TableTemplateWidget[]>;
  public abstract readonly tableGroupTemplateWidgets: Signal<
    TableGroupTemplateWidget[]
  >;

  public abstract markGroupReferenceIdAsDeleted(referenceId: string): void;
  public abstract isGroupReferenceIdDeleted(referenceId: string): boolean;
}
