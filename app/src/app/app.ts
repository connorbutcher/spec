import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SideNav } from './core/layout/side-nav/side-nav';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, SideNav],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
