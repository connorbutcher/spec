import { PropertyControlType } from '../../../core/types/properties/property-control-type.enum';
import { CopyFromPropertyControlComponent } from '../components/controls/copy-from-property-control/copy-from-property-control';
import { IntegerPropertyControl } from '../components/controls/integer-property-control/integer-property-control';
import { TextPropertyControl } from '../components/controls/text-property-control/text-property-control';
import { VersionPropertyControl } from '../components/controls/version-property-control/version-property-control';
import { NumberDisplayPropertyControl } from '../components/display/number-display-property-control/number-display-property-control';
import { TextDisplayPropertyControl } from '../components/display/text-display-property-control/text-display-property-control';

export class PropertyControlTypeHelpers {
  private static readonly _propertyControlTypeToEditComponentMap: Map<
    PropertyControlType,
    any
  > = new Map<PropertyControlType, any>([
    [PropertyControlType.Version, VersionPropertyControl],
    [PropertyControlType.Text, TextPropertyControl],
    [PropertyControlType.CopyFrom, CopyFromPropertyControlComponent],
    [PropertyControlType.Int, IntegerPropertyControl],
  ]);
  private static readonly _propertyControlTypeToViewComponentMap: Map<
    PropertyControlType,
    any
  > = new Map<PropertyControlType, any>([
    [PropertyControlType.Version, NumberDisplayPropertyControl],
    [PropertyControlType.Text, TextDisplayPropertyControl],
    [PropertyControlType.CopyFrom, NumberDisplayPropertyControl],
    [PropertyControlType.Int, NumberDisplayPropertyControl],
  ]);

  public static getEditComponent(
    propertyControlType: PropertyControlType,
  ): any {
    return this._propertyControlTypeToEditComponentMap.get(propertyControlType);
  }

  public static getViewComponent(
    propertyControlType: PropertyControlType,
  ): any {
    return this._propertyControlTypeToViewComponentMap.get(propertyControlType);
  }
}
