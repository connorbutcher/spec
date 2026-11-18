import { TreeNode } from 'primeng/api';
import { SpecSheetTypeStructureLevel } from '../../../core/models/domain/configuration/spec-sheet-type-structure-level';
import { SpecSheetPhase } from '../../../core/models/domain/types/spec-sheet-phase';
import { StructureLevelType } from '../../../core/types/configuration/structure-level-type.enum';
import { SpecSheetTreeNodeData } from '../models/spec-sheet-tree-node-data';
import { SpecSheetTreeRowData } from '../models/spec-sheet-tree-row-data';
import { SheetPropertyHelpers } from '../../../core/helpers/sheet-property-helpers';
import { SheetListItem, SheetProperty } from '../../../core/models/domain/sheets/sheet';

export class SpecSheetTreeHelpers {
  public static build(
    structureLevels: SpecSheetTypeStructureLevel[],
    specSheets: SheetListItem[],
    specSheetPhases: SpecSheetPhase[],
  ): TreeNode[] {
    const treeNodes: TreeNode[] = [];
    for (const phase of specSheetPhases) {
      const phaseNode: TreeNode = {
        data: phase,
        expanded: false,
        label: phase.phase,
        children: [],
        type: 'phase',
        key: `phase-${phase.id}`,
      };

      const groupLevels = structureLevels.filter(
        (sl) => sl.structureLevelType === StructureLevelType.Group,
      );
      const displayLevel = structureLevels.find(
        (sl) => sl.structureLevelType === StructureLevelType.Display,
      );
      if (!displayLevel) {
        continue; // We need a display level to show spec sheets
      }

      const specSheetsForPhase = specSheets.filter(
        (ss) => ss.specSheetPhaseId === phase.id,
      );

      // Recursively build the tree from groupLevels
      const children = this.buildLevelNodes(
        specSheetsForPhase,
        groupLevels,
        0,
        displayLevel,
      );
      phaseNode.children = children;

      treeNodes.push(phaseNode);
    }

    return treeNodes;
  }

  public static buildForCopyFrom(
    structureLevels: SpecSheetTypeStructureLevel[],
    specSheets: SheetListItem[],
    specSheetPhases: SpecSheetPhase[],
    allPhases: SpecSheetPhase[],
  ): TreeNode[] {
    const groupLevels = structureLevels.filter(
      (sl) => sl.structureLevelType === StructureLevelType.Group,
    );
    const displayLevel = structureLevels.find(
      (sl) => sl.structureLevelType === StructureLevelType.Display,
    );
    if (!displayLevel) {
      return []; // We need a display level to show spec sheets
    }

    const specSheetPhaseIds = new Set(specSheetPhases.map((p) => p.id));
    const treeNodes: TreeNode[] = [];
    for (const phase of allPhases) {
      const phaseNodesToRender: TreeNode[] = [];

      if (specSheetPhaseIds.has(phase.id)) {
        phaseNodesToRender.push(
          this.buildCopyFromPhaseNode(
            phase,
            specSheets,
            groupLevels,
            displayLevel,
          ),
        );
      }

      for (const childPhase of phase.phases) {
        if (!specSheetPhaseIds.has(childPhase.id)) {
          continue;
        }

        phaseNodesToRender.push(
          this.buildCopyFromPhaseNode(
            childPhase,
            specSheets,
            groupLevels,
            displayLevel,
          ),
        );
      }

      if (phaseNodesToRender.length === 0) {
        continue;
      }

      if (phase.phases.length > 0) {
        const parentPhaseNode: TreeNode = {
          data: phase,
          expanded: false,
          label: phase.phase,
          children: phaseNodesToRender,
          type: 'phase',
          key: `phase-parent-${phase.id}`,
        };

        treeNodes.push(parentPhaseNode);
      } else {
        treeNodes.push(...phaseNodesToRender);
      }
    }

    return treeNodes;
  }

  private static buildCopyFromPhaseNode(
    phase: SpecSheetPhase,
    specSheets: SheetListItem[],
    groupLevels: SpecSheetTypeStructureLevel[],
    displayLevel: SpecSheetTypeStructureLevel,
  ): TreeNode {
    const specSheetsForPhase = specSheets.filter(
      (ss) => ss.specSheetPhaseId === phase.id,
    );

    return {
      data: phase,
      expanded: false,
      label: phase.phase,
      children: this.buildLevelNodes(
        specSheetsForPhase,
        groupLevels,
        0,
        displayLevel,
        true, // For copy from, we want the data to be the spec sheet ID for easier lookup
      ),
      type: 'phase',
      key: `phase-${phase.id}`,
    };
  }

  private static buildLevelNodes(
    specSheets: SheetListItem[],
    groupLevels: SpecSheetTypeStructureLevel[],
    levelIndex: number,
    displayLevel: SpecSheetTypeStructureLevel,
    isDataId = false,
  ): TreeNode[] {
    if (levelIndex >= groupLevels.length) {
      // Base case: no more group levels, create leaf nodes for display level
      const specSheetLabels: {
        label: string;
        specSheet: SheetListItem;
        properties: SheetProperty[];
      }[] = [];
      for (const specSheet of specSheets) {
        let specSheetString = '';
        const propertiesToDisplay: SheetProperty[] = [];
        for (const levelValue of displayLevel.levelValues) {
          const property = SheetPropertyHelpers.findByReferenceId(
            specSheet.properties,
            specSheet.specSheetType?.properties ?? [],
            levelValue.specSheetTypePropertyReferenceId,
          );
          const propertyValue = SheetPropertyHelpers.getText(property);
          specSheetString += specSheetString
            ? ` - ${propertyValue}`
            : propertyValue;

          if (property) {
            propertiesToDisplay.push(property);
          }
        }

        specSheetLabels.push({
          label: specSheetString || 'N/A',
          specSheet,
          properties: propertiesToDisplay,
        });
      }

      return specSheetLabels
        .sort((x, y) => {
          switch (displayLevel.sortOrientation) {
            case 0:
              return x.label.localeCompare(y.label, undefined, {
                numeric: true,
              });
            case 1:
            default:
              return y.label.localeCompare(x.label, undefined, {
                numeric: true,
              });
          }
        })
        .map(({ label, specSheet, properties }) => ({
          data: isDataId
            ? specSheet.version?.sourceSpecSheetId
            : ({ specSheet, properties } as SpecSheetTreeRowData),
          expanded: false,
          label,
          children: [],
          type: 'specSheet',
          key: `${specSheet.sheetId}`,
        }));
    }

    const structureLevel = groupLevels[levelIndex];
    const groupLevelValue = structureLevel.levelValues[0]; // We only support single value group levels

    // Group the specSheets by the current structure level
    const groupedSpecSheetsByLevel = new Map<any, SpecSheetTreeNodeData>();
    for (const specSheet of specSheets) {
      const property = SheetPropertyHelpers.findByReferenceId(
        specSheet.properties,
        specSheet.specSheetType?.properties ?? [],
        groupLevelValue.specSheetTypePropertyReferenceId,
      );
      const propertyValue = SheetPropertyHelpers.getText(property, 'N/A');
      const existingGroup = groupedSpecSheetsByLevel.get(propertyValue);

      if (existingGroup) {
        existingGroup.specSheets.push(specSheet);
      } else {
        groupedSpecSheetsByLevel.set(propertyValue, {
          specSheets: [specSheet],
          property: property!,
        });
      }
    }

    const levelNodes: TreeNode[] = [];
    const groupKeys = Array.from(groupedSpecSheetsByLevel.keys()).sort(
      (x, y) => {
        switch (structureLevel.sortOrientation) {
          case 0:
            return typeof x === 'string' && typeof y === 'string'
              ? x.localeCompare(y, undefined, { numeric: true })
              : x - y;
          case 1:
          default:
            return typeof y === 'string' && typeof x === 'string'
              ? y.localeCompare(x, undefined, { numeric: true })
              : y - x;
        }
      },
    );

    for (const key of groupKeys) {
      const treeNodeData = groupedSpecSheetsByLevel.get(key);
      if (!treeNodeData) {
        continue;
      }

      const levelNode: TreeNode = {
        data: treeNodeData,
        expanded: false,
        label: key,
        children: this.buildLevelNodes(
          treeNodeData.specSheets,
          groupLevels,
          levelIndex + 1,
          displayLevel,
        ),
        type: 'group',
        key: `${structureLevel.id}-${key}`,
      };

      levelNodes.push(levelNode);
    }

    return levelNodes;
  }
}
