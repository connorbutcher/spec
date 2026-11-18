import { moveItemInArray } from '@angular/cdk/drag-drop';
import {
  computed,
  inject,
  Injector,
  runInInjectionContext,
  signal,
  Signal,
  WritableSignal,
} from '@angular/core';
import { UpdateHelpers } from '../../../core/helpers/update-helpers';
import { TableGroupTemplate } from '../../../core/models/domain/template/table-group-template';
import { SpecSheetOp } from '../../../core/models/operations/spec-sheet-op';
import { CellLookupService } from '../../../core/services/cell-lookup.service';
import { SheetContext } from '../services/sheet-context';
import { ResetOption } from '../types/cell-reset-options';
import { PUSpecSheetTableCellBase } from './cells/pu-spec-sheet-table-cell-base';
import { ConfigurableCellGroup } from './configuration/configurable-cell-group';
import { PUSpecSheetTableRow } from './pu-spec-sheet-table-row';
import { AdvisoryAggregator } from './services/advisory-aggregator';
import { ChangeTracker } from './services/change-tracker';
import { WidgetTypeRegistry } from './services/widget-type-registry';
import { SpecSheetAdvisory } from './spec-sheet-advisory';
import { PUSpecSheetTableBase } from './tables/pu-spec-sheet-table-base';
import { WidgetPropertyBase } from './widgets/widget-base';
import {
  SheetExternalReference,
  SheetTableGroup,
  SheetTableRow,
  SheetWidget,
} from '../../../core/models/domain/sheets/sheet';

export type ColumnDefinitions = {
  count: number;
  definitionString: string;
};

export class PUSpecSheetTableGroup {
  public readonly id: number;
  public readonly order: WritableSignal<number>;
  public readonly parentGroupId: WritableSignal<number | undefined>;
  public readonly referenceId: Signal<string>;
  public readonly rows: WritableSignal<PUSpecSheetTableRow[]>;
  public readonly childGroups: WritableSignal<PUSpecSheetTableGroup[]>;
  public readonly tableGroupTemplate = computed<TableGroupTemplate | undefined>(
    () => {
      const group = this.group;
      if (!group) {
        return undefined;
      }

      return this._activeSheetService
        .tableGroupTemplates()
        .find((tg) => tg.id === group.tableGroupTemplateId);
    },
  );
  public readonly widgets = computed(() => {
    const widgets = this.group.widgets;
    const tableGroupTemplate = this.tableGroupTemplate();
    if (!tableGroupTemplate || !widgets || widgets.length === 0) {
      return [];
    }

    const widgetProperties: WidgetPropertyBase<SheetWidget>[] = [];
    for (const widget of widgets) {
      const tableTemplateWidget =
        tableGroupTemplate.tableGroupTemplateWidgets.find(
          (ttw) => ttw.id === widget.tableGroupTemplateWidgetId,
        );
      if (!tableTemplateWidget) {
        continue;
      }

      widgetProperties.push(
        runInInjectionContext(this._injector, () =>
          WidgetTypeRegistry.createGroupWidget(
            tableTemplateWidget.widgetType.key,
            widget,
            this,
          ),
        ),
      );
    }

    return widgetProperties;
  });
  public readonly columnDefinitions = computed(() => {
    const rows = this.rows();
    const childGroups = this.childGroups();

    let columnCount = 0;
    let definitionString = '';

    const columnNumbersSet = new Map<number, number>();
    for (const row of rows) {
      for (const cell of row.cells()) {
        if (!columnNumbersSet.has(cell.columnNumber())) {
          columnNumbersSet.set(
            cell.columnNumber(),
            cell.stylingConfiguration().width || 10,
          );
        }
      }
    }

    columnCount = columnNumbersSet.size;
    definitionString = Array.from(columnNumbersSet.values())
      .map((width) => `${width}em`)
      .join(' ');

    for (const childGroup of childGroups) {
      const childColumnNumbersSet = new Map<number, number>();
      for (const row of childGroup.rows()) {
        for (const cell of row.cells()) {
          if (!childColumnNumbersSet.has(cell.columnNumber())) {
            childColumnNumbersSet.set(
              cell.columnNumber(),
              cell.stylingConfiguration().width || 10,
            );
          }
        }
      }

      columnCount += childColumnNumbersSet.size;
      definitionString +=
        ' ' +
        Array.from(childColumnNumbersSet.values())
          .map((width) => `${width}em`)
          .join(' ');
    }

    return { count: columnCount, definitionString };
  });
  public readonly identifierCells = computed(() => {
    const cells: PUSpecSheetTableCellBase[] = [];
    cells.push(...this.rows().flatMap((r) => r.identifierCells()));

    cells.push(...this.childGroups().flatMap((cg) => cg.identifierCells()));

    return cells;
  });
  public readonly configurableCells = computed(() => {
    return this.getConfigurableCells([], this);
  });
  public readonly references = computed(() => {
    const groupReferences = this.groupReferences();
    const rowReferences = this.rows().flatMap((r) => r.rowReferences());

    const references: SheetExternalReference[] = [
      ...groupReferences,
      ...rowReferences,
    ];
    for (const childGroup of this.childGroups()) {
      const childGroupReferences = childGroup.references();
      references.push(...childGroupReferences);
    }

    return references;
  });
  public readonly groupReferences = computed(() => {
    const externalReferences = this._activeSheetService.externalReferences();
    return externalReferences.filter(
      (er) => er.referenceId === this.referenceId(),
    );
  });

  public get isNew(): boolean {
    return this.group.id <= 0;
  }
  public readonly isValid = computed(() => this.advisories().length === 0);
  public readonly hasChanges!: Signal<boolean>;
  private _changeTracker!: ChangeTracker<SheetTableGroup>;
  // Own advisories: rules that belong to this group node only
  private readonly _ownAdvisories = computed<SpecSheetAdvisory[]>(() =>
    this.widgets().flatMap((w) => w.advisories()),
  );
  private readonly _rowAdvisories = computed<SpecSheetAdvisory[]>(() =>
    this.rows().flatMap((r) => r.advisories()),
  );
  private readonly _childGroupAdvisories = computed<SpecSheetAdvisory[]>(() =>
    this.childGroups().flatMap((g) => g.advisories()),
  );

  // Aggregated advisories: own + all children (rows, child groups)
  public readonly advisories = AdvisoryAggregator.aggregate(
    this._ownAdvisories,
    [this._rowAdvisories, this._childGroupAdvisories],
  );
  public readonly internalReferences = computed(() => {
    const references: SheetExternalReference[] = [];

    const childGroups = this.childGroups().flatMap((cg) =>
      cg.internalReferences(),
    );
    references.push(...childGroups);

    const rows = this.rows().flatMap((r) => r.internalReferences());
    references.push(...rows);
    return references;
  });

  private readonly _activeSheetService = inject(SheetContext);
  private readonly _cellLookupService = inject(CellLookupService);
  private readonly _injector = inject(Injector);

  private readonly _originalParentGroupId: number | undefined;
  private _childGroups: SheetTableGroup[] = [];

  constructor(
    public readonly group: SheetTableGroup,
    public readonly linkedGroups: SheetTableGroup[],
    private readonly _parent: PUSpecSheetTableBase,
    parentGroupId?: number,
  ) {
    this.id = group.id;
    this._originalParentGroupId = parentGroupId;
    this.parentGroupId = signal(parentGroupId);
    this.referenceId = signal(group.referenceId);
    this.order = signal(group.displayOrder);
    this.rows = signal(group.rows.map((r) => new PUSpecSheetTableRow(r, this)));

    // Get linked groups
    this._childGroups = linkedGroups.filter(
      (lg) => lg.linkedGroupReferenceId === group.referenceId,
    );

    this._changeTracker = new ChangeTracker(group).track(
      'displayOrder',
      this.order,
    );
    this.hasChanges = computed(() => {
      const hasDeletedRows = this.group.rows.some(
        (or) => !this.rows().find((r) => r.id === or.id),
      );
      const hasDeletedChildGroups = this._childGroups.some(
        (og) => !this.childGroups().find((g) => g.id === og.id),
      );
      return (
        this._changeTracker.hasChanges() ||
        this.parentGroupId() !== this._originalParentGroupId ||
        this.rows().some((c) => c.hasChanges() || c.isNew) ||
        this.childGroups().some((g) => g.hasChanges() || g.isNew) ||
        this.widgets().some((w) => w.hasChanges()) ||
        hasDeletedRows ||
        hasDeletedChildGroups
      );
    });

    const groups: PUSpecSheetTableGroup[] = [];
    for (const lg of this._childGroups) {
      const childGroups = linkedGroups.filter(
        (cg) => cg.linkedGroupReferenceId === lg.referenceId,
      );

      groups.push(
        new PUSpecSheetTableGroup(lg, childGroups, this._parent, this.id),
      );
    }

    this.childGroups = signal(groups);

    this._cellLookupService.registerGroup(this.referenceId(), this);
  }

  public addGroup(group: SheetTableGroup, index?: number): void {
    runInInjectionContext(this._injector, () => {
      const puGroup = new PUSpecSheetTableGroup(
        group,
        [],
        this._parent,
        this.id,
      );
      if (index !== undefined) {
        this.childGroups.update((groups) => {
          groups.splice(index, 0, puGroup);
          return [...groups];
        });
      } else {
        this.childGroups.update((groups) => [...groups, puGroup]);
      }
    });

    // Update orders
    for (let i = 0; i < this.childGroups().length; i++) {
      const group = this.childGroups()[i];
      group.order.set(i + 1);
    }
  }

  public addExistingGroup(group: PUSpecSheetTableGroup, index?: number): void {
    if (index !== undefined) {
      this.childGroups.update((groups) => {
        groups.splice(index, 0, group);
        return [...groups];
      });
    } else {
      this.childGroups.update((groups) => [...groups, group]);
    }

    // Update orders
    for (let i = 0; i < this.childGroups().length; i++) {
      const grp = this.childGroups()[i];
      grp.order.set(i + 1);
    }
  }

  public moveGroup(oldIndex: number, newIndex: number): void {
    this.childGroups.update((groups) => {
      moveItemInArray(groups, oldIndex, newIndex);
      return [...groups];
    });

    for (let i = 0; i < this.childGroups().length; i++) {
      const group = this.childGroups()[i];
      group.order.set(i + 1);
    }
  }

  public removeGroup(
    group: PUSpecSheetTableGroup,
    isBeingMoved: boolean,
  ): void {
    this.childGroups.update((groups) => groups.filter((g) => g !== group));

    if (!group.isNew && !isBeingMoved) {
      this._activeSheetService.markGroupReferenceIdAsDeleted(
        group.referenceId(),
      );
    }

    // Update orders
    for (let i = 0; i < this.childGroups().length; i++) {
      const grp = this.childGroups()[i];
      grp.order.set(i + 1);
    }

    this._cellLookupService.removeGroup(group.referenceId());

    this.remove(group);
  }

  public addRow(row: SheetTableRow, index?: number): void {
    runInInjectionContext(this._injector, () => {
      const puRow = new PUSpecSheetTableRow(row, this);
      if (index !== undefined) {
        this.rows.update((rows) => {
          rows.splice(index, 0, puRow);
          return [...rows];
        });
      } else {
        this.rows.update((rows) => [...rows, puRow]);
      }
    });

    // Update row numbers
    for (let i = 0; i < this.rows().length; i++) {
      const row = this.rows()[i];
      row.rowNumber.set(i + 1);
    }
  }

  public removeRow(row: PUSpecSheetTableRow): void {
    this.rows.update((rows) => rows.filter((r) => r !== row));

    // Update row numbers
    for (let i = 0; i < this.rows().length; i++) {
      const r = this.rows()[i];
      r.rowNumber.set(i + 1);
    }
  }

  public moveRow(oldIndex: number, newIndex: number): void {
    this.rows.update((rows) => {
      moveItemInArray(rows, oldIndex, newIndex);
      return [...rows];
    });

    for (let i = 0; i < this.rows().length; i++) {
      const row = this.rows()[i];
      row.rowNumber.set(i + 1);
    }
  }

  private remove(group: PUSpecSheetTableGroup): void {
    for (const row of group.rows()) {
      row.remove();
    }

    for (const childGroup of group.childGroups()) {
      childGroup.remove(childGroup);
    }
  }

  public getConfigurableCells(
    cellGroups: ConfigurableCellGroup[],
    group: PUSpecSheetTableGroup,
  ): ConfigurableCellGroup[] {
    for (const childGroup of group.childGroups()) {
      this.getConfigurableCells(cellGroups, childGroup);
    }

    for (const row of group.rows()) {
      cellGroups.push(...row.configurableCells());
    }

    return cellGroups;
  }

  public *compileOperations(): Generator<SpecSheetOp> {
    if (this.isNew) {
      yield {
        $type: 'AddSpecSheetTableGroupOp',
        tempId: this.id,
        specSheetTableId: this._parent.id,
        tableGroupTemplateId: this.group.tableGroupTemplateId,
        linkedSpecSheetTableGroupId: this.parentGroupId(),
        referenceId: this.referenceId(),
        order: this.order(),
      };
    } else if (this._changeTracker.hasChanges()) {
      yield {
        $type: 'UpdateSpecSheetTableGroupOp',
        id: this.id,
        order: UpdateHelpers.compare(this.group.displayOrder, this.order()),
        linkedSpecSheetTableGroupId: UpdateHelpers.compareToPropertyChanged(
          this._originalParentGroupId,
          this.parentGroupId(),
        ),
      };
    }

    for (const widget of this.widgets()) {
      yield* widget.compileOperations();
    }

    for (const row of this.rows()) {
      yield* row.compileOperations();
    }

    for (const childGroup of this.childGroups()) {
      yield* childGroup.compileOperations();
    }

    yield* this.compileDeleteOperations();
  }

  private *compileDeleteOperations(): Generator<SpecSheetOp> {
    for (const originalGroup of this._childGroups) {
      const exists = this.childGroups().find((g) => g.id === originalGroup.id);
      const isMarkedAsDeleted =
        this._activeSheetService.isGroupReferenceIdDeleted(
          originalGroup.referenceId,
        );
      if (!exists && isMarkedAsDeleted) {
        yield {
          $type: 'DeleteSpecSheetTableGroupOp',
          id: originalGroup.id,
        };
      }
    }

    for (const originalRow of this.group.rows) {
      const exists = this.rows().find((c) => c.id === originalRow.id);
      if (!exists) {
        yield {
          $type: 'DeleteSpecSheetTableRowOp',
          id: originalRow.id,
        };
      }
    }
  }
}
