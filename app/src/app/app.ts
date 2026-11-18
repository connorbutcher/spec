import { Component, OnInit, Signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Toast } from 'primeng/toast';
import { User } from './core/models/domain/access/user';
import { BrowserCommunicationProcessorService } from './core/services/browser-communication-processor.service';
import { UserService } from './core/services/user.service';
import { PUCardAnalyticsBanner } from './features/analytics/components/pu-card-analytics-banner/pu-card-analytics-banner';
import { SideNav } from './features/side-nav/side-nav';
import { UpdateService } from './features/updater/update.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, SideNav, Toast, PUCardAnalyticsBanner],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  constructor(
    private readonly _userService: UserService,
    private readonly _updateService: UpdateService,
    private readonly _browserCommunicationProcessorService: BrowserCommunicationProcessorService,
  ) {}

  ngOnInit(): void {
    this._updateService.startUpdateCheckCycle();
  }

  public get user(): Signal<User | undefined> {
    return this._userService.user;
  }
}
