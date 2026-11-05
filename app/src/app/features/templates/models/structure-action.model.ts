/** Something that can be done to the selected part of a table, shown as a button or a menu item. */
export type StructureAction = {
  /** Stable name, such as `moveUp`, for keyboard shortcuts and tests. */
  id: string;
  label: string;
  /** A PrimeIcons class, such as `pi-plus`. */
  icon: string;
  disabled: boolean;
  /** True for a delete, which is shown apart and in red. */
  danger: boolean;
  /** True when the icon says enough on a toolbar (moves and inserts); the label becomes its tooltip. */
  iconOnly: boolean;
  /** Runs the action. `target` is the element it was started from, which anchors a confirm popup. */
  run: (target: EventTarget | null) => void;
};
