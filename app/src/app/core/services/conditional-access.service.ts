import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ConditionalAccessService {
  public readonly updated = new Subject<void>();

  private _conditionalRoles = new Set<string>();

  public has(role: string | string[]): boolean {
    if (Array.isArray(role)) {
      return role.some((r) => this._conditionalRoles.has(r));
    }

    return this._conditionalRoles.has(role);
  }

  public set(...roles: string[]) {
    this._conditionalRoles.clear();

    for (const role of roles) {
      this.addRole(role);
    }

    this.updated.next();
  }

  public add(role: string) {
    this.addRole(role);
    this.updated.next();
  }

  public getAll(): string[] {
    return [...this._conditionalRoles.keys()];
  }

  public clear() {
    this._conditionalRoles.clear();
    this.updated.next();
  }

  private addRole(role: string) {
    if (!this._conditionalRoles.has(role)) {
      this._conditionalRoles.add(role);
    }
  }
}
