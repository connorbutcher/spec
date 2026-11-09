import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SheetSubmission } from './models/sheet-submission.model';

/** The decisions on a submission. The lists are read through `httpResource` in `ApprovalsStore`. */
@Service()

export class ApprovalsApi {
  private readonly http = inject(HttpClient);

  public approve(submissionId: number): Promise<SheetSubmission> {
    return firstValueFrom(this.http.post<SheetSubmission>(`/api/submissions/${submissionId}/approve`, {}));
  }

  public reject(submissionId: number, reason: string): Promise<SheetSubmission> {
    return firstValueFrom(this.http.post<SheetSubmission>(`/api/submissions/${submissionId}/reject`, { reason }));
  }

  public withdraw(submissionId: number): Promise<SheetSubmission> {
    return firstValueFrom(this.http.post<SheetSubmission>(`/api/submissions/${submissionId}/withdraw`, {}));
  }
}
