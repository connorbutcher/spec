import { GroupHierarchyManager } from '../models/services/group-hierarchy-manager';
import { SheetTableGroup } from '../../../core/models/domain/sheets/sheet';

export class SpecSheetTableGroupHelpers {
  /**
   * Recursively gets all groups including the current group and all nested child groups
   * @param rootGroup The root group to start traversal from
   * @param childGroups All available groups to traverse
   * @param includeRoot Whether to include the root group in the result (default: true)
   * @returns An array containing the root group and all its descendants
   */
  public static getAllGroupsRecursive(
    rootGroup: SheetTableGroup,
    childGroups: SheetTableGroup[],
    includeRoot: boolean = true,
  ): SheetTableGroup[] {
    const groupMap = new Map<number, SheetTableGroup>(
      childGroups.map((g) => [g.id, g]),
    );
    const idByReferenceId = new Map<string, number>(
      childGroups.map((g) => [g.referenceId, g.id]),
    );
    const hierarchyMap = GroupHierarchyManager.buildHierarchyMap(
      childGroups,
      (g) =>
        g.linkedGroupReferenceId
          ? idByReferenceId.get(g.linkedGroupReferenceId)
          : undefined,
    );
    const descendants = GroupHierarchyManager.getDescendants(
      rootGroup.id,
      hierarchyMap,
      groupMap,
    );
    return includeRoot ? [rootGroup, ...descendants] : descendants;
  }
}
