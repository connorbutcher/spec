import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { User } from '../../../core/models/domain/access/user';

@Injectable()
export class OAuthCallbackDataService {
  constructor(private readonly _httpClient: HttpClient) {}

  /**
   * Gets the current user.
   * @returns The current user.
   */
  public async getCurrentUserAsync(): Promise<User> {
    const url = `${environment.apiUrl}/users/current`;
    return await lastValueFrom(this._httpClient.get<User>(url));
  }
}
