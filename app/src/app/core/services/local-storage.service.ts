import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LocalStorageService {
  /**
   * Get the local storage item value with the specified key.
   * @param key The key of the local storage item to get.
   * @returns The value of the local storage item.
   */
  public get(key: string): string | undefined | null {
    if (!key) {
      return undefined;
    }

    return localStorage.getItem(key);
  }

  /**
   * Set a local storage item using a key and value.
   * @param key The key of the local storage item to set.
   * @param value The value of the local storage item to set.
   */
  public set(key: string, value: string): void {
    if (!key || !value) {
      return;
    }

    localStorage.setItem(key, value);
  }

  /**
   * Remove the local storage entry with the specified key.
   * @param key The key of the local storage item to remove.
   */
  public remove(key: string): void {
    if (!key) {
      return;
    }

    localStorage.removeItem(key);
  }
}
