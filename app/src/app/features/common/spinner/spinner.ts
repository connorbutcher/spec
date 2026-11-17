import { Component, input } from '@angular/core';

@Component({
  selector: 'app-spinner',
  templateUrl: './spinner.html',
  styleUrl: './spinner.scss',
})
export class Spinner {
  public readonly message = input.required<string>();
  public readonly hint = input.required<string>();
}
