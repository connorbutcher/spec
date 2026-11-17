import { Injectable, signal } from '@angular/core';

@Injectable()
export class PropertiesInfoService {
  public readonly extraPropertyInfo = signal<Map<string, any>>(new Map<string, any>());

  public addExtraInfo<T>(key: string, value: T): void {
    this.extraPropertyInfo.update((map) => {
      map.set(key, value);
      return new Map(map);
    });
  }

  public getExtraInfo<T>(key: string): T | undefined {
    return this.extraPropertyInfo().get(key) as T | undefined;
  }
}
