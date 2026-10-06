/**
 * The fill a cell takes from where its section sits: the header, a group at its depth (0 is the top
 * level, and anything deeper than 2 counts as 2), or plain data.
 */
export type SectionTone = 'plain' | 'header' | 'group-0' | 'group-1' | 'group-2';
