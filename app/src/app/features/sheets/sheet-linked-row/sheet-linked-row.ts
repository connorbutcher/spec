import { Component, input } from '@angular/core';
import { SheetLinkedValue } from '../models/sheet-linked-value.model';

/**
 * The other values of the row a linked dropdown's choice was picked from, written small under the choice:
 * each value after its label, wrapping over as many lines as they need. They are read from the source
 * row, so they follow it and can't be edited here.
 */
@Component({
  selector: 'app-sheet-linked-row',
  templateUrl: './sheet-linked-row.html',
  styleUrl: './sheet-linked-row.scss',
  host: {
    role: 'list',
    'aria-label': 'Values of the picked row',
  },
})
export class SheetLinkedRow {
  public readonly values = input.required<readonly SheetLinkedValue[]>();
}
