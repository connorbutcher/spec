import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UserList } from './components/user-list/user-list';
import { PermissionsService } from './services/permissions.service';

@Component({
  selector: 'app-permissions',
  imports: [RouterOutlet, UserList],
  templateUrl: './permissions.html',
  styleUrl: './permissions.scss',
  providers: [PermissionsService],
})
export class Permissions {}
