import { CellKind } from './cell-kind';
import { CellKindInfo } from './cell-kind-info.model';

/** Every cell kind, in the order they're offered. */
export const CELL_KINDS: readonly CellKindInfo[] = [
  {
    kind: 'Label',
    label: 'Label',
    icon: 'pi-align-left',
    description: 'Fixed text, such as a header.',
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
    kind: 'Dropdown',
    label: 'Dropdown',
    icon: 'pi-list',
    description: 'One choice from a list of options.',
  },
];

/** Display details for a cell kind. */
export function cellKindInfo(kind: CellKind): CellKindInfo {
  return CELL_KINDS.find((info) => info.kind === kind) ?? CELL_KINDS[1];
}
