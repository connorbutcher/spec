import { Component, Signal } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { ManageSpecSheetTypeService } from '../../../../admin/features/types/dialogs/services/manage-spec-sheet-type.service';
import { Properties } from '../../../models/properties.class';
import { Property } from '../../../models/property.class';
import { EditProperty } from '../edit-property/edit-property';
import { PropertyList } from '../property-list/property-list';

@Component({
  selector: 'app-edit-properties',
  imports: [PropertyList, EditProperty, ButtonDirective, Tag],
  templateUrl: './edit-properties.html',
  styleUrl: './edit-properties.scss',
})
export class EditProperties {
  constructor(
    private readonly _manageSpecSheetTypeService: ManageSpecSheetTypeService,
  ) {}

  public async deleteProperty() {
    this._manageSpecSheetTypeService
      .properties()
      .removeProperty(this.editingProperty()!.referenceId);
    this._manageSpecSheetTypeService.editingProperty.set(undefined);
  }

  public get editingProperty(): Signal<Property | undefined> {
    return this._manageSpecSheetTypeService.editingProperty;
  }

  public get properties(): Signal<Properties> {
    return this._manageSpecSheetTypeService.properties;
  }

  public get isValid(): Signal<boolean> {
    return this._manageSpecSheetTypeService.properties().isValid;
  }

  public get hasChanges(): Signal<boolean> {
    return this._manageSpecSheetTypeService.properties().hasChanges;
  }
}
