import { PublishScope } from './publish-scope';

/** One of the choices of what to publish, with how many changes it would send out. */
export interface PublishScopeOption {
  scope: PublishScope;
  label: string;
  count: number;
  /** A second line saying what the choice takes in, or why it can't be chosen. */
  detail: string | null;
  disabled: boolean;
}
