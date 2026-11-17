import { Injectable, signal } from '@angular/core';

export type PermissionMode = 'byuser' | 'bytype';

@Injectable()
export class PermissionsService {
  public readonly activeMode = signal<PermissionMode | undefined>(undefined);
  public readonly activeId = signal<number | undefined>(undefined);
}
