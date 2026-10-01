import { PanelLinkItem } from './models/panel-link-item.model';
import { TableTemplate } from './models/table-template.model';
import { copiesTag } from './instance-counts.util';

/** A table's column blocks as panel links, left to right, with how many copies each one has. */
export function columnBlockLinks(template: TableTemplate | null): PanelLinkItem[] {
  return [...(template?.columnBlocks ?? [])]
    .sort((a, b) => a.displayOrder - b.displayOrder)
    .map((block) => ({
      ref: { kind: 'columnBlock', id: block.id },
      label: block.name,
      icon: 'pi-pause',
      meta: copiesTag(block),
    }));
}
