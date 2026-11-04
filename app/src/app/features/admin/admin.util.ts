import { TreeNode } from 'primeng/api';
import { Phase } from '../../core/models/phase.model';
import { SheetType } from '../../core/models/sheet-type.model';
import { PhaseLocation } from './models/phase-location.model';
import { PhaseOption } from './models/phase-option.model';
import { SheetTypeUsage } from './models/sheet-type-usage.model';

const PATH_SEPARATOR = ' › ';

export class AdminUtil {
  /** Every phase in tree order as a flat list, each labelled with its path from the top level. */
  public static phaseOptions(tree: readonly TreeNode<Phase>[]): PhaseOption[] {
    const options: PhaseOption[] = [];
    AdminUtil.addPhaseOptions(tree, [], options);
    return options;
  }

  /** Each sheet type, in the order given, with the phases that have it in tree order. */
  public static sheetTypeUsage(sheetTypes: readonly SheetType[], tree: readonly TreeNode<Phase>[]): SheetTypeUsage[] {
    const phases: Phase[] = [];
    AdminUtil.addPhases(tree, phases);

    return sheetTypes.map((sheetType) => ({
      sheetType,
      phaseCodes: phases
        .filter((phase) => phase.sheetTypeIds.includes(sheetType.id))
        .map((phase) => phase.code),
    }));
  }

  /** Where a phase's node sits in the tree as it stands, or null when the tree doesn't hold it. */
  public static phaseLocation(tree: readonly TreeNode<Phase>[], phaseId: number): PhaseLocation | null {
    return AdminUtil.findPhase(tree, null, phaseId);
  }

  /** Whether two lists of ids hold the same ids, in any order. */
  public static sameIds(a: readonly number[], b: readonly number[]): boolean {
    if (a.length !== b.length) {
      return false;
    }
    const inB = new Set(b);
    return a.every((id) => inB.has(id));
  }

  private static addPhaseOptions(nodes: readonly TreeNode<Phase>[], path: string[], options: PhaseOption[]): void {
    for (const node of nodes) {
      if (!node.data) {
        continue;
      }
      const ownPath = [...path, node.data.code];
      options.push({ id: node.data.id, label: ownPath.join(PATH_SEPARATOR) });
      AdminUtil.addPhaseOptions(node.children ?? [], ownPath, options);
    }
  }

  private static addPhases(nodes: readonly TreeNode<Phase>[], phases: Phase[]): void {
    for (const node of nodes) {
      if (node.data) {
        phases.push(node.data);
      }
      AdminUtil.addPhases(node.children ?? [], phases);
    }
  }

  private static findPhase(
    nodes: readonly TreeNode<Phase>[],
    parentPhaseId: number | null,
    phaseId: number,
  ): PhaseLocation | null {
    for (const [index, node] of nodes.entries()) {
      if (node.data?.id === phaseId) {
        return { parentPhaseId, position: index + 1 };
      }
      const found = AdminUtil.findPhase(node.children ?? [], node.data?.id ?? null, phaseId);
      if (found) {
        return found;
      }
    }
    return null;
  }
}

