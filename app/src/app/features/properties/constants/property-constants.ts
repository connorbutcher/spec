import { MinMaxNumberValidatorProperty } from '../../../core/models/domain/configuration/validator-properties/min-max-number-validator-property';
import { SpecSheetTypeProperty } from '../../../core/models/domain/types/spec-sheet-type-property';
import { PropertyControlType } from '../../../core/types/properties/property-control-type.enum';
import { PropertyValidatorType } from '../../../core/types/properties/property-validator-type.enum';
import { PropertyValueType } from '../../../core/types/properties/property-value-type.enum';

export class PropertyConstants {
  public static readonly defaultRequiredProperties: SpecSheetTypeProperty[] = [
    {
      id: 0,
      referenceId: `${crypto.randomUUID()}`,
      valueName: 'Type',
      valueKey: 'type',
      valueControlType: PropertyControlType.Type,
      valueType: PropertyValueType.Int,
      canValueBeSetInForm: true,
      displayOrder: 1,
      specSheetTypeId: 0,
      placeholderText: 'The type of sheet',
      saveValueAsProperty: false,
      validators: [
        {
          validator: PropertyValidatorType.Required,
          message: 'Type is required',
        },
      ],
    },
    {
      id: 0,
      referenceId: `${crypto.randomUUID()}`,
      valueName: 'Phase',
      valueKey: 'phase',
      valueControlType: PropertyControlType.Phase,
      valueType: PropertyValueType.Int,
      canValueBeSetInForm: true,
      displayOrder: 2,
      specSheetTypeId: 0,
      placeholderText: 'The phase of sheet',
      saveValueAsProperty: false,
      validators: [
        {
          validator: PropertyValidatorType.Required,
          message: 'Phase is required',
        },
      ],
    },
  ];

  public static readonly defaultProperties: SpecSheetTypeProperty[] = [
    ...PropertyConstants.defaultRequiredProperties,
    {
      id: 0,
      referenceId: `${crypto.randomUUID()}`,
      valueName: 'Version',
      valueKey: 'version',
      valueControlType: PropertyControlType.Version,
      valueType: PropertyValueType.Int,
      canValueBeSetInForm: true,
      displayOrder: 3,
      specSheetTypeId: 0,
      saveValueAsProperty: true,
      placeholderText: 'The version of sheet',
      validators: [
        {
          validator: PropertyValidatorType.Required,
          message: 'Version is required',
        },
        {
          validator: PropertyValidatorType.MinNumber,
          message: 'The version must be at least 1',
          property: {
            $type: 'minMaxNumber',
            number: 1,
          } as MinMaxNumberValidatorProperty,
        },
      ],
    },
    {
      id: 0,
      referenceId: `${crypto.randomUUID()}`,
      valueName: 'Copy From',
      valueKey: 'copyFrom',
      valueControlType: PropertyControlType.CopyFrom,
      valueType: PropertyValueType.Int,
      canValueBeSetInForm: true,
      displayOrder: 4,
      specSheetTypeId: 0,
      saveValueAsProperty: true,
      placeholderText: 'The sheet to copy from',
    },
  ];
}
