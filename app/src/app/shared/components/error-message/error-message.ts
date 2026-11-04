import { Component, input, output } from '@angular/core';
import { MessageModule } from 'primeng/message';

/** A small PrimeNG error message, optionally with a close button that asks to dismiss it. */
@Component({
  selector: 'app-error-message',
  imports: [MessageModule],
  templateUrl: './error-message.html',
  styleUrl: './error-message.scss',
})
export class ErrorMessage {
  public readonly message = input.required<string>();
  public readonly closable = input(false);

  public readonly dismissed = output();
}
