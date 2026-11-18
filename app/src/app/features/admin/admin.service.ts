import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, resource } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Permission } from '../../core/models/domain/access/permission';
import { User } from '../../core/models/domain/access/user';
import { SheetTable } from '../../core/models/domain/sheets/sheet-table';

@Injectable()
export class AdminService {
  private readonly _httpClient = inject(HttpClient);

  public readonly permissions = computed(() =>
    (this.permissionsResource.value() ?? []).sort((x, y) =>
      x.title.localeCompare(y.title),
    ),
  );
  public readonly users = computed(() =>
    (this.usersResource.value() ?? []).sort((x, y) =>
      x.displayName.localeCompare(y.displayName),
    ),
  );
  public readonly specSheetTables = computed(() =>
    (this.specSheetTablesResource.value() ?? []).sort((x, y) =>
      x.title.localeCompare(y.title),
    ),
  );

  public readonly permissionsResource = resource({
    loader: () => this.getAllPermissionsAsync(),
  });
  public readonly usersResource = resource({
    loader: () => this.getAllUsersAsync(),
  });
  public readonly specSheetTablesResource = resource({
    loader: () => this.getAllUniqueTablesAsync(),
  });

  private async getAllPermissionsAsync(): Promise<Permission[]> {
    const url = `${environment.apiUrl}/permissions/all`;
    return await lastValueFrom(this._httpClient.get<Permission[]>(url));
  }

  private async getAllUsersAsync(): Promise<User[]> {
    const url = `${environment.apiUrl}/users/all`;
    return await lastValueFrom(this._httpClient.get<User[]>(url));
  }

  private async getAllUniqueTablesAsync(): Promise<SheetTable[]> {
    const url = `${environment.apiUrl}/sheets/tables/unique`;
    return await lastValueFrom(this._httpClient.get<SheetTable[]>(url));
  }
}
