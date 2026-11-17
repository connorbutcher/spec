import { Dialog } from '@angular/cdk/dialog';
import { Component, Signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Divider } from 'primeng/divider';
import { Tooltip } from 'primeng/tooltip';
import { LocalStorageConstants } from '../../core/constants/local-storage-constants';
import { ConditionalAccessService } from '../../core/services/conditional-access.service';
import { LocalStorageService } from '../../core/services/local-storage.service';
import { adminRoles } from '../admin/admin.routes';
import { PUCardAnalyticsService } from '../analytics/services/pu-card-analytics.service';
import { Help } from '../common/help/help';
import { SideNavPhaseTree } from './components/side-nav-phase-tree/side-nav-phase-tree';
import { SideNavService } from './side-nav.service';

@Component({
  selector: 'app-side-nav',
  imports: [RouterLink, RouterLinkActive, Divider, Tooltip, SideNavPhaseTree],
  templateUrl: './side-nav.html',
  styleUrl: './side-nav.scss',
})
export class SideNav {
  public readonly canViewAdmin: boolean;

  constructor(
    private readonly _sideNavService: SideNavService,
    private readonly _localStorageService: LocalStorageService,
    private readonly _conditionalAccessService: ConditionalAccessService,
    private readonly _puCardAnalyticsService: PUCardAnalyticsService,
    private readonly _router: Router,
    private readonly _dialog: Dialog,
  ) {
    this._sideNavService.setExpandedState(
      this._localStorageService.get(LocalStorageConstants.SIDENAV_EXPANDED) ===
        'true',
    );
    this.canViewAdmin = this._conditionalAccessService.has(adminRoles[0]);
  }

  public openHelpDialog(): void {
    this._dialog.open(Help);
  }

  public goHome(): void {
    this._router.navigate(['']);
  }

  public toggleExpanded(): void {
    this._sideNavService.setExpandedState(!this._sideNavService.isExpanded());
  }

  public get isExpanded(): Signal<boolean> {
    return this._sideNavService.isExpanded;
  }

  public get isAnalyticsEnabled(): Signal<boolean> {
    return this._puCardAnalyticsService.isEnabled;
  }
}
