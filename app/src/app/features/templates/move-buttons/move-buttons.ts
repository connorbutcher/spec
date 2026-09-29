import { Component, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

/** Up and down buttons for an item's 1-based position among its siblings. Emits the new position. */
@Component({
  selector: 'app-move-buttons',
  imports: [ButtonModule, TooltipModule],
  templateUrl: './move-buttons.html',
  styleUrl: './move-buttons.scss',
})
export class MoveButtons {
  public readonly position = input.required<number>();
  public readonly count = input.required<number>();
  public readonly disabled = input(false);
  /** What the buttons move, for screen readers, e.g. "section". */
  public readonly noun = input('item');

  public readonly moved = output<number>();

  public canMoveUp(): boolean {
    return !this.disabled() && this.position() > 1;
  }

  public canMoveDown(): boolean {
    return !this.disabled() && this.position() < this.count();
  }

  public moveUp(): void {
    this.moved.emit(this.position() - 1);
  }

  public moveDown(): void {
    this.moved.emit(this.position() + 1);
  }
}
