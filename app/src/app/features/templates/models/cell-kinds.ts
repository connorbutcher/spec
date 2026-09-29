import { CellKind } from './cell-kind';
import { CellKindInfo } from './cell-kind-info.model';

/** Every cell kind, in the order they're offered. */
export const CELL_KINDS: readonly CellKindInfo[] = [
  {
    kind: 'Heading',
    label: 'Heading',
    icon: 'pi-align-left',
    description: 'Fixed text, such as a column header.',
  },
  {
    kind: 'Group',
    label: 'Group',
    icon: 'pi-objects-column',
    description: 'A caption that groups the cells around it.',
  },
  { kind: 'Text', label: 'Text', icon: 'pi-pencil', description: 'Free text.' },
  {
    kind: 'Number',
    label: 'Number',
    icon: 'pi-hashtag',
    description: 'A number, with optional limits and unit.',
  },
  { kind: 'Date', label: 'Date', icon: 'pi-calendar', description: 'A date.' },
  { kind: 'Checkbox', label: 'Checkbox', icon: 'pi-check-square', description: 'Ticked or not.' },
  {
    kind: 'TextDropdown',
    label: 'Text dropdown',
    icon: 'pi-list',
    description: 'One choice from a list of text options.',
  },
  {
    kind: 'NumberDropdown',
    label: 'Number dropdown',
    icon: 'pi-sort-numeric-down',
    description: 'One choice from a list of numbers.',
  },
];

/** Display details for a cell kind. */
export function cellKindInfo(kind: CellKind): CellKindInfo {
  return CELL_KINDS.find((info) => info.kind === kind) ?? CELL_KINDS[2];
}

/** Whether cells of this kind are only shown, and never filled in on a sheet. */
export function isDisplayOnly(kind: CellKind | undefined): boolean {
  return kind === 'Heading' || kind === 'Group';
}

/** Whether cells of this kind pick one of the cell type's options. */
export function isDropdown(kind: CellKind | undefined): boolean {
  return kind === 'TextDropdown' || kind === 'NumberDropdown';
}
