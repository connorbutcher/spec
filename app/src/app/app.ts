import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ApprovalsNotice } from './core/layout/approvals-notice/approvals-notice';
import { SideNav } from './core/layout/side-nav/side-nav';

@Component({
  selector: 'app-root',
  imports: [ApprovalsNotice, RouterOutlet, SideNav],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
