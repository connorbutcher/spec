import { TreeNode } from 'primeng/api';
import { Phase } from '../../core/models/phase.model';
import { SheetType } from '../../core/models/sheet-type.model';
import { PhaseOption } from './models/phase-option.model';
import { SheetTypeUsage } from './models/sheet-type-usage.model';

const PATH_SEPARATOR = ' › ';

/** Every phase in tree order as a flat list, each labelled with its path from the top level. */
export function phaseOptions(tree: readonly TreeNode<Phase>[]): PhaseOption[] {
  const options: PhaseOption[] = [];
  const visit = (nodes: readonly TreeNode<Phase>[], path: string[]): void => {
    for (const node of nodes) {
      if (!node.data) {
        continue;
      }
      const ownPath = [...path, node.data.code];
      options.push({ id: node.data.id, label: ownPath.join(PATH_SEPARATOR) });
      visit(node.children ?? [], ownPath);
    }
  };
  visit(tree, []);
  return options;
}

/** Each sheet type, in the order given, with the phases that have it in tree order. */
export function sheetTypeUsage(
  sheetTypes: readonly SheetType[],
  tree: readonly TreeNode<Phase>[],
): SheetTypeUsage[] {
  const phases: Phase[] = [];
  const visit = (nodes: readonly TreeNode<Phase>[]): void => {
    for (const node of nodes) {
      if (node.data) {
        phases.push(node.data);
      }
      visit(node.children ?? []);
    }
  };
  visit(tree);

  return sheetTypes.map((sheetType) => ({
    sheetType,
    phaseCodes: phases
      .filter((phase) => phase.sheetTypeIds.includes(sheetType.id))
      .map((phase) => phase.code),
  }));
}

/** Whether two lists of ids hold the same ids, in any order. */
export function sameIds(a: readonly number[], b: readonly number[]): boolean {
  if (a.length !== b.length) {
    return false;
  }
  const inB = new Set(b);
  return a.every((id) => inB.has(id));
}
