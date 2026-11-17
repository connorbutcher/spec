import { uniqBy } from 'lodash';
import { TreeNode } from 'primeng/api';
import { SpecSheetPhase } from '../models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../models/domain/types/spec-sheet-type';

export class SpecSheetPhaseHelpers {
  public static getUniqueSpecSheetTypes(
    phase: SpecSheetPhase,
  ): SpecSheetType[] {
    const types: SpecSheetType[] = [];
    for (const type of phase.types) {
      types.push(type);
    }

    for (const subPhase of phase.phases) {
      for (const type of subPhase.types) {
        types.push(type);
      }
    }

    return uniqBy(types, (ut) => ut.id).sort(
      (x, y) => x.displayOrder - y.displayOrder,
    );
  }

  public static getSpecSheetPhasesBySpecSheetTypeId(
    phases: SpecSheetPhase[],
    typeId: number,
  ): SpecSheetPhase[] {
    const result: SpecSheetPhase[] = [];
    for (const phase of phases) {
      if (phase.types.some((t) => t.id === typeId)) {
        result.push(phase);
      }

      const subPhases = this.getSpecSheetPhasesBySpecSheetTypeId(
        phase.phases,
        typeId,
      );
      result.push(...subPhases);
    }
    return result;
  }

  public static getPhaseByIdFromArray(
    phases: SpecSheetPhase[],
    phaseId: number,
  ): SpecSheetPhase | undefined {
    for (const phase of phases) {
      const found = this.getPhaseById(phase, phaseId);
      if (found) {
        return found;
      }
    }

    return undefined;
  }

  public static getParentPhaseByIdFromArray(
    phases: SpecSheetPhase[],
    phaseId: number,
  ): SpecSheetPhase | undefined {
    for (const phase of phases) {
      const found = this.getPhaseById(phase, phaseId);
      if (found) {
        return phase;
      }
    }

    return undefined;
  }

  public static indexTreeNodes(
    nodes: readonly TreeNode<SpecSheetPhase>[],
  ): Map<number, TreeNode<SpecSheetPhase>> {
    const index = new Map<number, TreeNode<SpecSheetPhase>>();
    const visit = (node: TreeNode<SpecSheetPhase>): void => {
      if (node.data) {
        index.set(node.data.id, node);
      }

      node.children?.forEach(visit);
    };

    nodes.forEach(visit);
    return index;
  }

  private static getPhaseById(
    phase: SpecSheetPhase,
    phaseId: number,
  ): SpecSheetPhase | undefined {
    if (phase.id === phaseId) {
      return phase;
    }

    for (const subPhase of phase.phases) {
      const found = this.getPhaseById(subPhase, phaseId);
      if (found) {
        return found;
      }
    }

    return undefined;
  }
}
