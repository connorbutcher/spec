import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { RowTakeover } from './models/row-takeover.model';

/** Asking to take over a row that is checked out to someone else, and answering such a request. */
@Service()
export class RowTakeoversApi {
  private readonly http = inject(HttpClient);

  public request(rowId: number): Promise<RowTakeover> {
    return firstValueFrom(
      this.http.post<RowTakeover>(`/api/sheet-rows/${rowId}/takeover-requests`, null),
    );
  }

  public approve(takeoverId: string): Promise<RowTakeover> {
    return firstValueFrom(
      this.http.post<RowTakeover>(`/api/row-takeovers/${takeoverId}/approve`, null),
    );
  }

  public deny(takeoverId: string): Promise<RowTakeover> {
    return firstValueFrom(
      this.http.post<RowTakeover>(`/api/row-takeovers/${takeoverId}/deny`, null),
    );
  }

  public cancel(takeoverId: string): Promise<RowTakeover> {
    return firstValueFrom(this.http.delete<RowTakeover>(`/api/row-takeovers/${takeoverId}`));
  }
}
