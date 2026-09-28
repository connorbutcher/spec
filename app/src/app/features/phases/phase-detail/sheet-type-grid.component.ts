import { Component, input } from '@angular/core';
import { SheetType } from '../../../core/models/sheet-type.model';
import { SheetTypeCardComponent } from './sheet-type-card.component';

/** The sheet types available to a phase, as a grid of cards. */
@Component({
  selector: 'app-sheet-type-grid',
  imports: [SheetTypeCardComponent],
  template: `
    @if (sheetTypes().length > 0) {
      <ul class="sheet-type-grid">
        @for (sheetType of sheetTypes(); track sheetType.id) {
          <li>
            <app-sheet-type-card [sheetType]="sheetType" />
          </li>
        }
      </ul>
    } @else {
      <p class="none">No sheet types have been made available to this phase yet.</p>
    }
  `,
  styles: `
    .sheet-type-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(190px, 1fr));
      gap: 12px;
      margin: 0;
      padding: 0;
      list-style: none;
    }

    .none {
      margin: 0;
      color: #64748b;
      font-size: 0.875rem;
    }
  `,
})
export class SheetTypeGridComponent {
  public readonly sheetTypes = input.required<SheetType[]>();
}
