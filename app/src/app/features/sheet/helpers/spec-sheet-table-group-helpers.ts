import { SpecSheetTableGroup } from '../../../core/models/domain/spec-sheet/spec-sheet-table-group';
import { GroupHierarchyManager } from '../models/services/group-hierarchy-manager';

export class SpecSheetTableGroupHelpers {
  /**
   * Recursively gets all groups including the current group and all nested child groups
   * @param rootGroup The root group to start traversal from
   * @param childGroups All available groups to traverse
   * @param includeRoot Whether to include the root group in the result (default: true)
   * @returns An array containing the root group and all its descendants
   */
  public static getAllGroupsRecursive(
    rootGroup: SpecSheetTableGroup,
    childGroups: SpecSheetTableGroup[],
    includeRoot: boolean = true,
  ): SpecSheetTableGroup[] {
    const groupMap = new Map<number, SpecSheetTableGroup>(
      childGroups.map((g) => [g.id, g]),
    );
    const hierarchyMap = GroupHierarchyManager.buildHierarchyMap(
      childGroups,
      (g) => g.linkedSpecSheetTableGroupId ?? undefined,
    );
    const descendants = GroupHierarchyManager.getDescendants(
      rootGroup.id,
      hierarchyMap,
      groupMap,
    );
    return includeRoot ? [rootGroup, ...descendants] : descendants;
  }
}
