import { Component } from '@angular/core';

/** The heading of the status column: one cell down the whole of the table's header, pinned like the column. */
@Component({
  selector: 'app-sheet-status-heading',
  templateUrl: './sheet-status-heading.html',
  styleUrl: './sheet-status-heading.scss',
  host: {
    class: 'app-grid-cell',
    role: 'columnheader',
    'data-tone': 'header',
  },
})
export class SheetStatusHeading {}
