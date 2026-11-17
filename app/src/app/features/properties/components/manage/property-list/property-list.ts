import {
  CdkDrag,
  CdkDragDrop,
  CdkDropList,
  DragDropModule,
} from '@angular/cdk/drag-drop';
import { Component, computed, inject, Signal } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { ManageSpecSheetTypeService } from '../../../../admin/features/types/dialogs/services/manage-spec-sheet-type.service';
import { Property } from '../../../models/property.class';

@Component({
  selector: 'app-property-list',
  imports: [DragDropModule, ButtonDirective],
  templateUrl: './property-list.html',
  styleUrl: './property-list.scss',
})
export class PropertyList {
  public readonly properties = computed<Property[]>(
    () => this._manageSpecSheetTypeService.properties()?.properties() ?? [],
  );
  public readonly reorderableProperties = computed<Property[]>(() =>
    this.properties()
      .filter((p) => p.canBeEdited())
      .sort((x, y) => x.displayOrder() - y.displayOrder()),
  );
  public readonly nonReorderableProperties = computed<Property[]>(() =>
    this.properties().filter((p) => !p.canBeEdited()),
  );

  private readonly _manageSpecSheetTypeService = inject(
    ManageSpecSheetTypeService,
  );

  public addProperty(): void {
    const property = this._manageSpecSheetTypeService
      .properties()
      .addProperty();
    this._manageSpecSheetTypeService.editingProperty.set(property);
  }

  public editProperty(property: Property) {
    this._manageSpecSheetTypeService.editingProperty.set(property);
  }

  public drop(event: CdkDragDrop<Property[]>) {
    const previousIndex = this.reorderableProperties().findIndex(
      (item) => item === event.item.data,
    );
    this.reorderableProperties().splice(previousIndex, 1);
    this.reorderableProperties().splice(event.currentIndex, 0, event.item.data);

    this.reorderableProperties().forEach((property, index) =>
      property.displayOrder.set(index + 1),
    );
  }

  public sortPredicate(
    index: number,
    item: CdkDrag<Property>,
    dropList: CdkDropList<Property[]>,
  ) {
    const itemAtIndex = dropList.data[index];
    if (!itemAtIndex) {
      return false;
    }

    return itemAtIndex.canBeEdited();
  }

  public get editingProperty(): Signal<Property | undefined> {
    return this._manageSpecSheetTypeService.editingProperty;
  }
}
