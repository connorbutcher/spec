/** Where a request to take over a row has got to. Everything but `Pending` is final. */
export type RowTakeoverStatus =
  | 'Pending'
  | 'Approved'
  | 'Denied'
  | 'Cancelled'
  | 'GrantedOnTimeout'
  | 'GrantedHolderAway'
  | 'Released';
