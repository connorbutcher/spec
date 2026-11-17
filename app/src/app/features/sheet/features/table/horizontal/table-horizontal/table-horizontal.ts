import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import {
  Component,
  computed,
  effect,
  inject,
  input,
  Signal,
  signal,
} from '@angular/core';
import { Destroyable } from '../../../../../common/destructable/destructable';
import { PUSpecSheetTableBase } from '../../../../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from '../../../../services/active-sheet.service';
import { TableHeader } from '../../table-header/table-header';
import { TableGroupHorizontal } from '../table-group-horizontal/table-group-horizontal';

@Component({
  selector: 'app-table-horizontal',
  imports: [DragDropModule, TableHeader, TableGroupHorizontal],
  templateUrl: './table-horizontal.html',
  styleUrl: './table-horizontal.scss',
})
export class TableHorizontal extends Destroyable {
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly rebuildVersion = signal(0);
  public readonly groups = computed(() => {
    this.rebuildVersion();
    return [...this.table().groups()];
  });
  public readonly columnDefinitionString = computed(() => {
    return this.table().columnDefinitions().definitionString;
  });

  private readonly _activeSheetService = inject(ActiveSheetService);

  constructor() {
    super();

    effect((onCleanup) => {
      const subscription = this.table().triggerRebuild$.subscribe(() => {
        this.rebuildVersion.update((version) => version + 1);
      });
      onCleanup(() => subscription.unsubscribe());
    });
  }

  public drop(event: CdkDragDrop<any>): void {}

  public get canEdit(): Signal<boolean> {
    return this._activeSheetService.canEdit as any;
  }
}
