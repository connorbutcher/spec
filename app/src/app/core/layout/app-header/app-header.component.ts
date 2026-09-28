import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiStatusComponent } from '../api-status/api-status.component';

@Component({
  selector: 'app-header',
  imports: [RouterLink, ApiStatusComponent],
  templateUrl: './app-header.component.html',
  styleUrl: './app-header.component.scss',
})
export class AppHeaderComponent {}
