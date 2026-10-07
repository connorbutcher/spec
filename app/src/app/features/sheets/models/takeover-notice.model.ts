/** What to tell someone when a takeover request they are part of is settled. */
export interface TakeoverNotice {
  severity: 'success' | 'info' | 'warn';
  summary: string;
  detail: string;
  /** Stays on screen until dismissed: the reader lost a row and may not be looking. */
  sticky: boolean;
}
