import { computed, signal } from '@angular/core';
import { SpecSheetTypeProperty } from '../../../core/models/domain/types/spec-sheet-type-property';
import { CreateSpecSheetTypePropertyRequest } from '../../../core/models/requests/types/properties/create-spec-sheet-type-property-request';
import { ManageSpecSheetTypePropertiesRequest } from '../../../core/models/requests/types/properties/manage-spec-sheet-type-property-request';
import { UpdateSpecSheetTypePropertyRequest } from '../../../core/models/requests/types/properties/update-spec-sheet-type-property-request';
import { PropertyControlType } from '../../../core/types/properties/property-control-type.enum';
import { PropertyValueType } from '../../../core/types/properties/property-value-type.enum';
import { Property } from './property.class';

export class Properties {
  public readonly properties = signal<Property[]>([]);
  public readonly isValid = computed(() =>
    this.properties().every((property) => property.isValid()),
  );
  public readonly hasChanges = computed(() => {
    const properties = this.properties();
    const originalProperties = this.specSheetTypeProperties
      .filter((p) => p.saveValueAsProperty)
      .map((p) => p.id);
    const currentProperties = properties
      .filter((p) => !p.isNew && p.canBeEdited())
      .map((p) => p.id);
    const deletedProperties = originalProperties.filter(
      (id) => !currentProperties.some((p) => p === id),
    );

    return (
      this.properties().some((property) => property.hasChanges()) ||
      deletedProperties.length > 0
    );
  });

  constructor(
    public readonly specSheetTypeProperties: SpecSheetTypeProperty[],
  ) {
    this.properties.set(specSheetTypeProperties.map((p) => new Property(p)));
  }

  public getNewProperties(): CreateSpecSheetTypePropertyRequest[] {
    return this.properties()
      .filter((property) => property.isNew && property.canBeEdited())
      .map((property) => property.toNewPropertyRequest());
  }

  public getUpdatedProperties(): UpdateSpecSheetTypePropertyRequest[] {
    return this.properties()
      .filter(
        (property) =>
          !property.isNew && property.hasChanges() && property.canBeEdited(),
      )
      .map((property) => property.toUpdatePropertyRequest());
  }

  public getDeletedProperties(): number[] {
    const originalProperties = this.specSheetTypeProperties
      .filter((p) => p.saveValueAsProperty)
      .map((p) => p.id);
    const currentProperties = this.properties().filter(
      (p) => !p.isNew && p.canBeEdited(),
    );

    const deletedProperties = originalProperties.filter(
      (id) => !currentProperties.some((p) => p.id === id),
    );
    return deletedProperties;
  }

  public toManagePropertiesRequest(): ManageSpecSheetTypePropertiesRequest {
    return {
      specSheetTypeId:
        this.properties().length > 0
          ? this.properties()[0].property.specSheetTypeId
          : 0,
      createPropertyRequests: this.getNewProperties(),
      updatePropertyRequests: this.getUpdatedProperties(),
      deletePropertyRequests: this.getDeletedProperties(),
    };
  }

  public addProperty(): Property {
    const newProperty = new Property({
      id: -1,
      specSheetTypeId:
        this.properties().length > 0
          ? this.properties()[0].property.specSheetTypeId
          : 0,
      referenceId: crypto.randomUUID(),
      displayOrder: this.properties().length + 1,
      canValueBeSetInForm: true,
      valueControlType: PropertyControlType.Text,
      valueKey: '',
      valueName: '',
      valueType: PropertyValueType.String,
      saveValueAsProperty: true,
    });

    this.properties.update((properties) => [...properties, newProperty]);
    return newProperty;
  }

  public removeProperty(uiId: string): void {
    this.properties.update((properties) => {
      const index = properties.findIndex(
        (property) => property.referenceId === uiId,
      );
      if (index !== -1) {
        properties.splice(index, 1);
      }

      return [...properties];
    });
  }
}
