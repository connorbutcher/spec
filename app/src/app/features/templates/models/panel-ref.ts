/** What the configuration panel is showing. Each entry is one step in its back/forward history. */
export type PanelRef =
  | { kind: 'template' }
  | { kind: 'section'; id: number }
  | { kind: 'columnBlock'; id: number }
  | { kind: 'row'; id: number }
  | { kind: 'cell'; id: number }
  | { kind: 'cellTypes' }
  | { kind: 'cellType'; id: number };
