export class GroupHierarchyManager {
  /**
   * Build a group hierarchy map for efficient lookups.
   * @param groups Array of groups to index
   * @param getParentId Optional extractor for parent ID; defaults to reading `parentGroupId` (signal or plain value)
   */
  public static buildHierarchyMap<T extends { id: number }>(
    groups: T[],
    getParentId?: (group: T) => number | null | undefined,
  ): Map<number, number[]> {
    const parentToChildren = new Map<number, number[]>();

    for (const group of groups) {
      const parentId = getParentId
        ? getParentId(group)
        : ((group as any).parentGroupId?.() ?? (group as any).parentGroupId);
      if (parentId) {
        if (!parentToChildren.has(parentId)) {
          parentToChildren.set(parentId, []);
        }
        parentToChildren.get(parentId)!.push(group.id);
      }
    }

    return parentToChildren;
  }

  /**
   * Get all descendants of a group recursively
   */
  public static getDescendants(
    groupId: number,
    hierarchyMap: Map<number, number[]>,
    groupMap: Map<number, any>,
  ): any[] {
    const descendants: any[] = [];
    const childIds = hierarchyMap.get(groupId) ?? [];

    for (const childId of childIds) {
      const group = groupMap.get(childId);
      if (group) {
        descendants.push(group);
        descendants.push(
          ...this.getDescendants(childId, hierarchyMap, groupMap),
        );
      }
    }

    return descendants;
  }
}
