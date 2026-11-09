/**
 * Where a table, section, column block or row is on its way to being published. An item is in exactly
 * one state. `ForApproval` is a change submitted for publishing that waits for its approvers.
 */
export type ItemState = 'Published' | 'CheckedOut' | 'Draft' | 'ForApproval' | 'Publishing';
