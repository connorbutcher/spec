export const states = ['unchanged', 'added', 'removed', 'modified'] as const;
export type SpecSheetState = (typeof states)[number];
