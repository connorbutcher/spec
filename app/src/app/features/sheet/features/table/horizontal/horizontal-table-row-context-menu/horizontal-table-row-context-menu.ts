import { Dialog } from '@angular/cdk/dialog';
import {
  Component,
  ElementRef,
  HostListener,
  Renderer2,
  Signal,
  computed,
  effect,
  inject,
  viewChild,
} from '@angular/core';
import { Popover } from 'primeng/popover';
import { Tooltip } from 'primeng/tooltip';
import { firstValueFrom } from 'rxjs';
import { TableGroupTemplate } from '../../../../../../core/models/domain/template/table-group-template';
import { SystemReferencesConfirmationDialogData } from '../../../../dialogs/system-references-confirmation-dialog/models/system-references-confirmation-dialog-data.interface';
import { SystemReferencesConfirmationDialog } from '../../../../dialogs/system-references-confirmation-dialog/system-references-confirmation-dialog';
import { ActiveSheetService } from '../../../../services/active-sheet.service';
import { SheetCheckoutService } from '../../../../services/sheet-checkout.service';
import {
  ActiveElement,
  RowContextMenuService,
} from '../../../../services/row-context-menu.service';
import { TableGroupBuilderService } from '../../../../services/table-group-builder.service';

@Component({
  selector: 'app-horizontal-table-row-context-menu',
  templateUrl: './horizontal-table-row-context-menu.html',
  styleUrl: './horizontal-table-row-context-menu.scss',
  imports: [Tooltip, Popover],
})
export class HorizontalTableRowHoverContextMenu {
  public readonly popover = viewChild.required(Popover);

  @HostListener('mouseenter')
  onMouseEnter(): void {
    this._rowContextMenuService.setContextMenuHovered(true);
  }

  @HostListener('mouseleave')
  onMouseLeave(): void {
    if (this.popover().overlayVisible()) {
      return;
    }

    this._rowContextMenuService.setContextMenuHovered(false);
    this._rowContextMenuService.setActiveElement(undefined);
  }

  public readonly addOptions = computed(() => {
    const activeElement = this.activeElement();
    if (!activeElement) {
      return [];
    }

    const table = activeElement.table;
    const parentGroup = activeElement.parentGroup;
    const tableType = table.tableType();
    if (!tableType) {
      return [];
    }

    return (
      tableType.tableGroupTemplates.filter(
        (tgt) =>
          tgt.canUserAddToTable &&
          tgt.showInContextMenu &&
          tgt.parentTableGroupTemplateId ===
            parentGroup.tableGroupTemplate()?.id,
      ) ?? []
    );
  });

  /** A row can't be removed while someone else has it, or anything in it, checked out. */
  public readonly canRemove = computed(() => {
    const activeElement = this.activeElement();
    if (!activeElement) {
      return false;
    }

    const group = activeElement.group;
    return (
      this._specSheetService.canEditItem(group.group.revision) &&
      group
        .rows()
        .every((row) => this._specSheetService.canEditItem(row.row.revision))
    );
  });
  /** Whether the current user has saved changes to the row they can undo. */
  public readonly canUndo = computed(() => {
    const activeElement = this.activeElement();
    if (!activeElement || activeElement.group.id <= 0) {
      return false;
    }

    const group = activeElement.group;
    return [
      group.group.revision,
      ...group.rows().map((row) => row.row.revision),
    ].some((revision) => {
      const lock = this._specSheetService.lockOf(revision);
      return !!lock?.isMine && !lock.isAwaitingApproval;
    });
  });

  private _direction: 'above' | 'below' | undefined = undefined;

  private readonly _rowContextMenuService = inject(RowContextMenuService);
  private readonly _renderer = inject(Renderer2);
  private readonly _elementRef = inject(ElementRef<HTMLElement>);
  private readonly _specSheetService = inject(ActiveSheetService);
  private readonly _tableGroupBuilderService = inject(TableGroupBuilderService);
  private readonly _dialog = inject(Dialog);
  private readonly _sheetCheckoutService = inject(SheetCheckoutService);

  constructor() {
    effect(() => {
      const activeElement = this.activeElement();
      if (activeElement) {
        this._positionMenuNextToElement(activeElement.element);
      }
    });
  }

  public async addGroupAsync(
    tableGroupTemplate: TableGroupTemplate,
  ): Promise<void> {
    if (!this.canEdit()) {
      return;
    }

    const activeElement = this.activeElement();
    if (!activeElement) {
      return;
    }

    const row = activeElement.row;
    if (!row) {
      return;
    }

    const currentOrder = row.rowNumber();
    const newRowNumber =
      this._direction === 'above' ? currentOrder : currentOrder + 1;

    await this._tableGroupBuilderService.addOptionAsync(
      tableGroupTemplate,
      this.activeElement()!.table,
      this.activeElement()!.parentGroup,
      newRowNumber,
    );
  }

  public async undoAsync(): Promise<void> {
    const activeElement = this.activeElement();
    if (!activeElement || !this.canUndo()) {
      return;
    }

    await this._sheetCheckoutService.undoGroupAsync(activeElement.group.id);
  }

  public async removeGroupAsync(): Promise<void> {
    if (!this.canEdit() || !this.canRemove()) {
      return;
    }

    const activeElement = this.activeElement();
    if (!activeElement) {
      return;
    }

    if (
      activeElement.group.references() &&
      activeElement.group.references().length > 0
    ) {
      const dialogRef = this._dialog.open<
        boolean,
        SystemReferencesConfirmationDialogData,
        SystemReferencesConfirmationDialog
      >(SystemReferencesConfirmationDialog, {
        data: {
          type: 'Row',
          references: activeElement.group.references(),
        },
      });

      const result = await firstValueFrom(dialogRef.closed);
      if (!result) {
        return;
      }
    }

    const row = activeElement.row;
    if (!row) {
      return;
    }

    for (const group of activeElement.parentGroup.childGroups()) {
      const groupRow = group
        .rows()
        .find((r) => r.rowNumber() === row.rowNumber());
      if (groupRow) {
        group.removeRow(groupRow);
      }
    }

    activeElement.parentGroup.removeRow(row);
    activeElement.table.triggerRebuild();
  }

  public togglePopover(event: Event, direction: 'above' | 'below'): void {
    this._direction = direction;
    this.popover().toggle(event);
  }

  public onPopoverShow(): void {
    this._rowContextMenuService.setContextMenuHovered(true);
  }

  public onPopoverHide(): void {
    this._rowContextMenuService.setContextMenuHovered(false);
    this._rowContextMenuService.setActiveElement(undefined);
  }

  public get activeElement(): Signal<ActiveElement | undefined> {
    return this._rowContextMenuService.activeElement;
  }

  public get canEdit(): Signal<boolean> {
    return this._specSheetService.canEdit;
  }

  private _positionMenuNextToElement(element: HTMLTableRowElement): void {
    const rect = element.getBoundingClientRect();
    const menuElement = this._elementRef.nativeElement;

    const menuHeight = 75;
    const menuWidth = 36;
    const spacing = 8;

    // Find the scrollable container
    let scrollableContainer = element.parentElement;
    while (scrollableContainer && scrollableContainer !== document.body) {
      const overflowX = window.getComputedStyle(scrollableContainer).overflowX;
      if (overflowX === 'auto' || overflowX === 'scroll') {
        break;
      }
      scrollableContainer = scrollableContainer.parentElement;
    }

    const scrollLeft = scrollableContainer ? scrollableContainer.scrollLeft : 0;

    const top = rect.top + window.scrollY + rect.height / 2 - menuHeight / 2;

    // Display on the left, accounting for horizontal scroll
    const left = rect.left + window.scrollX - menuWidth - spacing + scrollLeft;
    this._renderer.addClass(menuElement, 'menu-left');

    this._renderer.setStyle(menuElement, 'top', `${top}px`);
    this._renderer.setStyle(menuElement, 'left', `${left}px`);
  }
}
