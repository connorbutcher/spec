import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { CurrentUserStore } from '../../../core/auth/current-user.store';
import { ApprovalsStore } from '../../approvals/approvals.store';
import { SheetSubmission } from '../../approvals/models/sheet-submission.model';
import { SheetSubmissionLine } from './sheet-submission-line';

const SUBMISSION: SheetSubmission = {
  id: 12,
  sheetId: 4,
  phaseId: 1,
  phaseCode: 'V6',
  sheetTypeId: 2,
  sheetTypeName: 'Parts',
  note: 'Rev B',
  submittedByUserId: 2,
  submittedByName: 'Engineer Two',
  submittedAtUtc: '2026-11-09T10:00:00Z',
  status: 'Pending',
  decidedAtUtc: null,
  versionNumber: null,
  changes: { tables: 0, sections: 0, rows: 1, columnBlocks: 0, total: 1 },
  approvers: [{ userId: 1, userName: 'Developer', decision: 'Pending', decidedAtUtc: null, reason: null }],
};

class SheetSubmissionLinePage {
  public fixture!: ComponentFixture<SheetSubmissionLine>;
  public readonly rejected: [number, string][] = [];

  public async render(userId: number): Promise<void> {
    const approvals = {
      busyId: signal<number | null>(null),
      error: signal<string | null>(null),
      approve: () => Promise.resolve(null),
      withdraw: () => Promise.resolve(null),
      reject: (id: number, reason: string) => {
        this.rejected.push([id, reason]);
        return Promise.resolve({ ...SUBMISSION, status: 'Rejected' as const });
      },
    };
    TestBed.configureTestingModule({
      imports: [SheetSubmissionLine],
      providers: [
        provideRouter([]),
        { provide: ApprovalsStore, useValue: approvals },
        { provide: CurrentUserStore, useValue: { user: signal({ id: userId }) } },
      ],
    });
    this.fixture = TestBed.createComponent(SheetSubmissionLine);
    this.fixture.componentRef.setInput('submission', SUBMISSION);
    await this.fixture.whenStable();
  }

  public buttons(): string[] {
    return [...(this.fixture.nativeElement as HTMLElement).querySelectorAll('button')]
      .map((button) => button.textContent?.trim() ?? '');
  }
}

describe('SheetSubmissionLine', () => {
  let page: SheetSubmissionLinePage;

  beforeEach(() => {
    page = new SheetSubmissionLinePage();
  });

  it('offers Review, Approve and Reject to an approver who has not decided', async () => {
    await page.render(1);
    expect(page.buttons()).toEqual(['Review', 'Approve', 'Reject']);
    expect((page.fixture.nativeElement as HTMLElement).textContent).toContain("Engineer Two's changes for approval");
  });

  it('offers Withdraw, not a decision, to the submitter', async () => {
    await page.render(2);
    expect(page.buttons()).toEqual(['Review', 'Withdraw']);
  });
});
