/**
 * Where a table, section, column block or row is on its way to being published. An item is in exactly
 * one state. `ForApproval` is reserved for an approval step that does not exist yet: nothing produces it.
 */
export type ItemState = 'Published' | 'CheckedOut' | 'Draft' | 'ForApproval' | 'Publishing';
