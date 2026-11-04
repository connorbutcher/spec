import { Component, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';

/** The small outlined "+ Add ..." button used under lists and in panel headers. */
@Component({
  selector: 'app-add-button',
  imports: [ButtonModule],
  templateUrl: './add-button.html',
  styleUrl: './add-button.scss',
})
export class AddButton {
  public readonly label = input.required<string>();
  public readonly disabled = input(false);

  public readonly clicked = output();
}
