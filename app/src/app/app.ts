import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppHeaderComponent } from './core/layout/app-header/app-header.component';
import { SideNavComponent } from './core/layout/side-nav/side-nav.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AppHeaderComponent, SideNavComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
