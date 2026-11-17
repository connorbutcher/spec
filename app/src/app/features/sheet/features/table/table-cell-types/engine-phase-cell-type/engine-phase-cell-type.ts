import { A11yModule } from '@angular/cdk/a11y';
import {
  Component,
  computed,
  HostListener,
  inject,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Select } from 'primeng/select';
import { filter, skip, takeUntil } from 'rxjs';
import { SpecSheetPhaseHelpers } from '../../../../../../core/helpers/spec-sheet-phase-helpers';
import { PhasesService } from '../../../../../../core/services/phases.service';
import { CellTypeBase } from '../cell-type-base';

@Component({
  selector: 'app-engine-phase-cell-type',
  imports: [FormsModule, A11yModule, Select],
  templateUrl: './engine-phase-cell-type.html',
  styleUrl: './engine-phase-cell-type.scss',
  host: { class: 'cell-host' },
})
export class EnginePhaseCellType extends CellTypeBase<number> {
  @HostListener('document:keydown', ['$event'])
  onGlobalKeydown(event: KeyboardEvent): void {
    if (event.key === 'Tab') {
      this._pendingKeyboardFocus = true;
    }
  }

  @HostListener('document:mouseup')
  @HostListener('document:mousedown')
  onGlobalPointer(): void {
    this._pendingKeyboardFocus = false;
  }

  public readonly dropdown = viewChild<Select>(Select);
  public readonly enginePhaseOptions = computed(() => {
    const phase = this._sheetService.specSheet().specSheetPhase();
    const phases = this._phasesService.specSheetPhases();
    const parentPhase = SpecSheetPhaseHelpers.getParentPhaseByIdFromArray(
      phases,
      phase.id,
    );
    if (!parentPhase) {
      return [];
    }

    return parentPhase.phases;
  });

  private readonly _phasesService = inject(PhasesService);

  private _pendingKeyboardFocus = false;

  constructor() {
    super();

    toObservable(this.cellValue)
      .pipe(skip(1), takeUntilDestroyed())
      .subscribe((value) => {
        this.cell().value.set(value);
      });
  }

  ngOnInit(): void {
    toObservable(this.cell().value, { injector: this._injector })
      .pipe(
        takeUntil(this.destroy),
        filter((value) => value !== this.cellValue()),
      )
      .subscribe(() => {
        this.cellValue.set(this.cell().value());
      });
  }

  public override isMatch(cellValue: any, filter: string): boolean {
    if (cellValue === null || cellValue === undefined) {
      return false;
    }

    const phase = this.enginePhaseOptions().find((p) => p.id === cellValue);
    if (!phase) {
      return false;
    }

    return phase.phase.toLocaleLowerCase().includes(filter);
  }

  public override focusCell(event: any): void {
    if (!this.canEdit()) {
      return;
    }

    if (!event) {
      this.isCellFocused.set(false);
      return;
    }

    this.isCellFocused.set(true);

    const dropdown = this.dropdown();
    if (!dropdown || !this.isCellFocused()) {
      return;
    }

    // Open only if focus arrived via Tab (flag set by global keydown)
    if (this._pendingKeyboardFocus && !dropdown.overlayVisible) {
      dropdown.show();
    }

    this._pendingKeyboardFocus = false; // reset after use
  }

  public onKeydown(event: KeyboardEvent): void {
    // Allow Enter / Space to toggle when already focused
    if ((event.key === 'Enter' || event.key === ' ') && this.isCellFocused()) {
      const dropdown = this.dropdown();
      if (dropdown && !dropdown.overlayVisible) dropdown.show();
      event.preventDefault();
    }
  }

  public onMousedown(): void {
    this._pendingKeyboardFocus = false;
  }
}
