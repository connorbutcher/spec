import { Component, input } from '@angular/core';
import { Spinner } from '../spinner/spinner';

@Component({
  selector: 'app-loading-block',
  imports: [Spinner],
  templateUrl: './loading-block.html',
  styleUrl: './loading-block.scss',
})
export class LoadingBlock {
  public readonly message = input.required<string>();
  public readonly hint = input.required<string>();
}
