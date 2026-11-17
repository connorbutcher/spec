import { computed, Signal } from '@angular/core';
import { UpdateHelpers } from '../../../../core/helpers/update-helpers';

export interface PropertyChange<T = any> {
  property: string;
  originalValue: T;
  currentValue: T;
}

export class ChangeTracker<T extends Record<string, any>> {
  private readonly _trackedSignals = new Map<string, Signal<any>>();

  constructor(private readonly _original: T) {}

  public track<K extends keyof T>(property: K, current: Signal<T[K]>): this {
    this._trackedSignals.set(property as string, current);
    return this;
  }

  public get hasChanges(): Signal<boolean> {
    return computed(() => {
      for (const [property, currentSignal] of this._trackedSignals.entries()) {
        const originalValue = (this._original as any)[property];
        if (!UpdateHelpers.isEqual(originalValue, currentSignal())) {
          return true;
        }
      }

      return false;
    });
  }
}
