import {
  computed,
  inject,
  Injector,
  runInInjectionContext,
  Signal,
  signal,
  WritableSignal,
} from '@angular/core';
import { isNil } from 'lodash';
import { Subject } from 'rxjs';
import { UpdateHelpers } from '../../../../core/helpers/update-helpers';
import { TableProperties } from '../../../../core/models/domain/properties/table/table-properties';
import { TableTemplate } from '../../../../core/models/domain/template/table-template';
import { TableOrientation } from '../../../../core/models/domain/types/spec-sheet-type';
import { SpecSheetOp } from '../../../../core/models/operations/spec-sheet-op';
import { CellLookupService } from '../../../../core/services/cell-lookup.service';
import { TypesService } from '../../../../core/services/types.service';
import { ValidatorLevel } from '../../../../core/types/configuration/validator-level.enum';
import { TablePropertiesClassHelpers } from '../../features/table-properties/helpers/table-properties-class-helper';
import { TablePropertyBase } from '../../features/table-properties/models/table-property-base';
import { SpecSheetTableGroupHelpers } from '../../helpers/spec-sheet-table-group-helpers';
import { SheetContext } from '../../services/sheet-context';
import { PUSpecSheet } from '../pu-spec-sheet';
import { PUSpecSheetTableGroup } from '../pu-spec-sheet-table-group';
import { PUSpecSheetTableNote } from '../pu-spec-sheet-table-note';
import { AdvisoryAggregator } from '../services/advisory-aggregator';
import { ChangeTracker } from '../services/change-tracker';
import { WidgetTypeRegistry } from '../services/widget-type-registry';
import { SpecSheetAdvisory } from '../spec-sheet-advisory';
import { WidgetPropertyBase } from '../widgets/widget-base';
import { SheetExternalReference } from '../../../../core/models/domain/sheets/sheet-external-reference';
import { SheetTable } from '../../../../core/models/domain/sheets/sheet-table';
import { SheetTableGroup } from '../../../../core/models/domain/sheets/sheet-table-group';
import { SheetTableNote } from '../../../../core/models/domain/sheets/sheet-table-note';
import { SheetWidget } from '../../../../core/models/domain/sheets/sheet-widget';

export abstract class PUSpecSheetTableBase<
  P extends TablePropertyBase<TableProperties> =
    TablePropertyBase<TableProperties>,
> {
  public readonly id: number;
  public readonly referenceId: Signal<string>;
  public readonly title: WritableSignal<string>;
  public readonly tableTemplateId: WritableSignal<number>;
  public readonly displayOrder: WritableSignal<number>;
  public readonly groups: WritableSignal<PUSpecSheetTableGroup[]>;
  public readonly properties = signal<P | undefined>(undefined);
  public readonly notes: WritableSignal<PUSpecSheetTableNote[]>;
  public readonly requiresUserConfiguration = computed(
    () => this.tableType().requiresUserConfiguration,
  );
  public readonly canUserEdit: boolean = true;
  public readonly headerColorClass: string =
    'var(--theme-table-border-color-red)';
  public readonly widgets = computed(() => {
    const widgets = this.table.widgets;
    const tableType = this.tableType();
    if (!tableType || !widgets || widgets.length === 0) {
      return [];
    }

    const widgetProperties: WidgetPropertyBase<SheetWidget>[] = [];
    for (const widget of widgets) {
      const tableTemplateWidget = tableType.tableTemplateWidgets.find(
        (ttw) => ttw.id === widget.tableTemplateWidgetId,
      );
      if (!tableTemplateWidget) {
        continue;
      }

      widgetProperties.push(
        runInInjectionContext(this._injector, () =>
          WidgetTypeRegistry.createTableWidget(
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
    return this.groups()[0].columnDefinitions();
  });
  public readonly triggerRebuild$ = new Subject<void>();
  public readonly tableType = computed<TableTemplate>(() => {
    const tableTypeId = this.tableTemplateId();
    return this._typesService
      .tableTemplates()
      .find((tt) => tt.id === tableTypeId)!;
  });
  public readonly orientation = computed<TableOrientation>(
    () => this._parent.specSheetType().tableOrientation,
  );
  public readonly isValid = computed(() => this.advisories().length === 0);
  public readonly hasChanges: Signal<boolean>;

  private readonly _ownAdvisories = computed<SpecSheetAdvisory[]>(() => {
    const advisories: SpecSheetAdvisory[] = [];

    for (const widget of this.widgets()) {
      advisories.push(...widget.advisories());
    }

    if (this.title().trim().length === 0) {
      advisories.push({
        level: ValidatorLevel.Error,
        message: 'Title is required.',
        referenceId: this.referenceId(),
      });
    }

    if (this.groups().length === 0) {
      advisories.push({
        level: ValidatorLevel.Error,
        message: 'At least one row is required.',
        referenceId: this.referenceId(),
      });
    }

    return advisories;
  });

  private readonly _groupAdvisories = computed<SpecSheetAdvisory[]>(() =>
    this.groups().flatMap((g) => g.advisories()),
  );
  private readonly _noteAdvisories = computed<SpecSheetAdvisory[]>(() =>
    this.notes()
      .flatMap((n) => n.advisories())
      .map((message) => ({
        level: ValidatorLevel.Error,
        message,
        referenceId: this.referenceId(),
      })),
  );

  // Aggregated advisories: own + all children (groups, notes)
  public readonly advisories = AdvisoryAggregator.aggregate(
    this._ownAdvisories,
    [this._groupAdvisories, this._noteAdvisories],
  );
  public readonly advisoryState = AdvisoryAggregator.getWorstState(
    this.advisories,
  );
  public readonly identifierCells = computed(() =>
    this.groups().flatMap((g) => g.identifierCells()),
  );
  public readonly configurableCells = computed(() =>
    this.groups().flatMap((g) => g.configurableCells()),
  );
  public readonly cellsNeedingConfiguration = computed(
    () =>
      this.configurableCells().filter((c) =>
        c.cells.some((c) => !c.properties()?.isValid()),
      ).length,
  );
  public readonly references = computed(() => {
    const tableReferences = this.tableReferences();
    const groupReferences = this.groups().flatMap((g) => g.references());

    return [...tableReferences, ...groupReferences];
  });
  public readonly tableReferences = computed(() => {
    const externalReferences = this._activeSheetService.externalReferences();
    return externalReferences.filter(
      (er) => er.referenceId === this.referenceId(),
    );
  });
  public readonly internalReferences = computed(() => {
    const references: SheetExternalReference[] = [];

    const properties = this.properties();
    if (
      properties &&
      properties.internalReferences() &&
      properties.internalReferences().length > 0
    ) {
      references.push(...properties.internalReferences());
    }

    return references;
  });

  private readonly _activeSheetService = inject(SheetContext);
  private readonly _cellLookupService = inject(CellLookupService);
  private readonly _injector = inject(Injector);
  private readonly _typesService = inject(TypesService);

  protected readonly _rootGroups: SheetTableGroup[];

  private _changeTracker!: ChangeTracker<SheetTable>;

  constructor(
    public readonly table: SheetTable,
    private readonly _parent: PUSpecSheet,
  ) {
    this.id = table.id;
    this.referenceId = signal(table.referenceId);
    this.title = signal(table.title);
    this.tableTemplateId = signal(table.tableTemplateId);
    this.displayOrder = signal(table.displayOrder);
    this.notes = signal(table.notes.map((n) => new PUSpecSheetTableNote(n)));

    this.properties.set(
      TablePropertiesClassHelpers.map(
        this.tableType(),
        this.table.configurationOverride,
      ) as P,
    );

    this._rootGroups = table.groups.filter((g) =>
      isNil(g.linkedGroupReferenceId),
    );
    this.groups = signal(
      this._rootGroups
        .sort((x, y) => x.displayOrder - y.displayOrder)
        .map((g) => {
          const linkedGroups = SpecSheetTableGroupHelpers.getAllGroupsRecursive(
            g,
            table.groups,
          );
          return new PUSpecSheetTableGroup(g, linkedGroups, this);
        }),
    );

    this._changeTracker = new ChangeTracker(table)
      .track('title', this.title)
      .track('displayOrder', this.displayOrder);
    this.hasChanges = computed(() => {
      const hasDeletedGroups = this._rootGroups.some(
        (og) => !this.groups().find((g) => g.id === og.id),
      );

      return (
        this._changeTracker.hasChanges() ||
        this.groups().some((g) => g.hasChanges() || g.isNew) ||
        this.notes().some((n) => n.hasChanges() || n.isNew) ||
        hasDeletedGroups
      );
    });

    this._cellLookupService.registerTable(this.referenceId(), this);
  }

  public triggerRebuild(): void {
    this.triggerRebuild$.next();
  }

  public addGroup(group: SheetTableGroup, index?: number): void {
    runInInjectionContext(this._injector, () => {
      const puGroup = new PUSpecSheetTableGroup(group, [], this);
      if (isNil(index)) {
        this.groups.update((groups) => [...groups, puGroup]);
      } else {
        this.groups.update((groups) => {
          const newGroups = [...groups];
          newGroups.splice(index, 0, puGroup);
          return newGroups;
        });
      }
    });

    for (let i = 0; i < this.groups().length; i++) {
      const grp = this.groups()[i];
      grp.order.set(i + 1);
    }
  }

  public moveGroup(fromIndex: number, toIndex: number): void {
    this.groups.update((groups) => {
      const newGroups = [...groups];
      if (
        fromIndex < 0 ||
        fromIndex >= newGroups.length ||
        toIndex < 0 ||
        toIndex >= newGroups.length
      ) {
        return newGroups;
      }

      const [movedGroup] = newGroups.splice(fromIndex, 1);
      newGroups.splice(toIndex, 0, movedGroup);
      return newGroups;
    });

    for (let i = 0; i < this.groups().length; i++) {
      const grp = this.groups()[i];
      grp.order.set(i + 1);
    }
  }

  public removeGroup(group: PUSpecSheetTableGroup): void {
    this.groups.update((groups) => {
      return groups.filter((g) => g.id !== group.id);
    });

    for (let i = 0; i < this.groups().length; i++) {
      const grp = this.groups()[i];
      grp.order.set(i + 1);
    }

    this._cellLookupService.removeGroup(group.referenceId());
  }

  public addExistingGroupAtIndex(
    group: PUSpecSheetTableGroup,
    index: number,
  ): void {
    this.groups.update((groups) => {
      const newGroups = [...groups];
      newGroups.splice(index, 0, group);
      return newGroups;
    });

    for (let i = 0; i < this.groups().length; i++) {
      const grp = this.groups()[i];
      grp.order.set(i + 1);
    }
  }

  public addNote(note: SheetTableNote): void {
    this.notes.update((notes) => [...notes, new PUSpecSheetTableNote(note)]);
  }

  public removeNote(note: PUSpecSheetTableNote): void {
    this.notes.update((notes) => notes.filter((n) => n !== note));
  }

  public *compileOperations(): Generator<SpecSheetOp> {
    if (this.isNew) {
      yield {
        $type: 'AddSpecSheetTableOp',
        tempId: this.id,
        referenceId: this.referenceId(),
        tableTemplateId: this.tableTemplateId(),
        title: this.title(),
        displayOrder: this.displayOrder(),
        configurationOverride: this.properties()?.toConfiguration(),
      };
    } else if (
      this._changeTracker.hasChanges() ||
      this.properties()?.hasChanges()
    ) {
      yield {
        $type: 'UpdateSpecSheetTableOp',
        id: this.id,
        title: UpdateHelpers.compare(this.table.title, this.title()),
        displayOrder: UpdateHelpers.compare(
          this.table.displayOrder,
          this.displayOrder(),
        ),
        configurationOverride: UpdateHelpers.compareToPropertyChanged(
          this.table.configurationOverride,
          this.properties()?.toConfiguration(),
        ),
      };
    }

    for (const widget of this.widgets()) {
      yield* widget.compileOperations();
    }

    for (const note of this.notes()) {
      yield* note.compileOperations();
    }

    for (const group of this.groups()) {
      yield* group.compileOperations();
    }

    yield* this.compileDeleteOperations();
  }

  private *compileDeleteOperations(): Generator<SpecSheetOp> {
    for (const originalGroup of this._rootGroups) {
      const exists = this.groups().find((g) => g.id === originalGroup.id);
      if (!exists) {
        yield {
          $type: 'DeleteSpecSheetTableGroupOp',
          id: originalGroup.id,
        };
      }
    }
  }

  public get isNew(): boolean {
    return this.table.id <= 0;
  }
}
