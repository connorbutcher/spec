import { inject, Injectable, Signal, signal } from '@angular/core';
import { LocalStorageConstants } from '../../core/constants/local-storage-constants';
import { LocalStorageService } from '../../core/services/local-storage.service';

@Injectable({ providedIn: 'root' })
export class SideNavService {
  private readonly _isExpanded = signal<boolean>(true);
  private readonly _localStorageService = inject(LocalStorageService);

  public setExpandedState(isExpanded: boolean): void {
    this._isExpanded.set(isExpanded);
    this._localStorageService.set(
      LocalStorageConstants.SIDENAV_EXPANDED,
      this._isExpanded() ? 'true' : 'false',
    );
  }

  public get isExpanded(): Signal<boolean> {
    return this._isExpanded;
  }
}
