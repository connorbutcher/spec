import { DragDropModule } from '@angular/cdk/drag-drop';
import { Component, input } from '@angular/core';

@Component({
  selector: 'app-dialog-header',
  templateUrl: './dialog-header.html',
  styleUrl: './dialog-header.scss',
  imports: [DragDropModule],
})
export class DialogHeader {
  public readonly headerText = input.required<string>();
  public readonly canDrag = input<boolean>(false);
}
