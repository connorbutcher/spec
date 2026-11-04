import { RowTakeover } from './models/row-takeover.model';

export class RowTakeoverFixture {
  /**
   * Test data: user 2 (Engineer Two) asking for row 10 on sheet 5, which is checked out to user 1
   * (Developer), asked at noon with a minute to answer.
   */
  public static fixtureTakeover(change: Partial<RowTakeover> = {}): RowTakeover {
    return {
      id: 'a',
      sheetId: 5,
      rowId: 10,
      requesterUserId: 2,
      requesterName: 'Engineer Two',
      holderUserId: 1,
      holderName: 'Developer',
      requestedAtUtc: '2026-10-07T12:00:00Z',
      expiresAtUtc: '2026-10-07T12:01:00Z',
      status: 'Pending',
      ...change,
    };
  }
}
