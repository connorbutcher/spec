import { Component, input } from '@angular/core';

/** A dropdown option drawn as a muted PrimeIcon before its label. */
@Component({
  selector: 'app-icon-option',
  templateUrl: './icon-option.html',
  styleUrl: './icon-option.scss',
})
export class IconOption {
  /** The PrimeIcon class, e.g. "pi-hashtag". */
  public readonly icon = input.required<string>();
  public readonly label = input.required<string>();
}
