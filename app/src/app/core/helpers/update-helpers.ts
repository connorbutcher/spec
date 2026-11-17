import { isEqualWith } from 'lodash';
import { PropertyChanged } from '../models/common/property-changed';

export class UpdateHelpers {
  public static isEqual<T>(originalValue: T, newValue: T): boolean {
    if (typeof newValue === 'string' && newValue.trim() === '') {
      newValue = undefined as T;
    }

    return (
      isEqualWith(originalValue, newValue, (originalX, newX) =>
        UpdateHelpers.equalWithFunc(originalX, newX),
      ) ?? false
    );
  }

  public static compare<T>(originalValue: T, newValue: T): T | undefined {
    if (typeof newValue === 'string' && newValue.trim() === '') {
      newValue = undefined as T;
    }

    const isEqual = isEqualWith(originalValue, newValue, (originalX, newX) =>
      UpdateHelpers.equalWithFunc(originalX, newX),
    );
    return isEqual ? undefined : newValue;
  }

  public static compareToPropertyChanged<T>(
    originalValue: T,
    newValue: T,
  ): PropertyChanged<T> {
    if (typeof newValue === 'string' && newValue.trim() === '') {
      newValue = undefined as T;
    }

    const isEqual = isEqualWith(originalValue, newValue, (originalX, newX) =>
      UpdateHelpers.equalWithFunc(originalX, newX),
    );
    return { hasChanged: !isEqual, value: newValue };
  }

  private static equalWithFunc(originalX: any, newX: any): boolean | undefined {
    // Treat undefined and null as the same. The standard isEqual treats them differently when inside an object.
    if (
      (originalX === undefined || originalX === null) &&
      (newX === undefined || newX === null)
    ) {
      return true;
    }

    return undefined;
  }
}
