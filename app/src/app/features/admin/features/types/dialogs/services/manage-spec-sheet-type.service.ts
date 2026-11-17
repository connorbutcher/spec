import { computed, Injectable, signal } from '@angular/core';
import { Properties } from '../../../../../properties/models/properties.class';
import { Property } from '../../../../../properties/models/property.class';
import { StructureLevels } from '../../../../../structure/models/structure-levels';
import { SpecSheetTypeBasicInfo } from '../models/spec-sheet-type-basic-info';

@Injectable()
export class ManageSpecSheetTypeService {
  public readonly basicInfo = signal<SpecSheetTypeBasicInfo>(undefined!);
  public readonly properties = signal<Properties>(undefined!);
  public readonly structureLevels = signal<StructureLevels>(undefined!);
  public readonly activeTab = signal<'general' | 'properties' | 'structure'>(
    'general',
  );
  public readonly editingProperty = signal<Property | undefined>(undefined);

  public readonly isValid = computed(
    () =>
      this.basicInfoValid() && this.propertiesValid() && this.structureValid(),
  );
  public readonly hasChanges = computed(
    () =>
      this.basicInfo().hasChanges() ||
      this.properties().hasChanges() ||
      this.structureLevels().hasChanges(),
  );

  public readonly basicInfoValid = computed(() => this.basicInfo().isValid());
  public readonly propertiesValid = computed(() => this.properties().isValid());
  public readonly structureValid = computed(() =>
    this.structureLevels().isValid(),
  );
}
