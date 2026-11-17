import { FocusMonitor } from '@angular/cdk/a11y';
import { AfterViewInit, Directive, ViewContainerRef } from '@angular/core';

@Directive({
  standalone: true,
  selector: '[appFocus]',
})
export class FocusDirective implements AfterViewInit {
  constructor(
    private vcRef: ViewContainerRef,
    private readonly _focusMonitor: FocusMonitor,
  ) {}

  ngAfterViewInit(): void {
    this._focusMonitor.focusVia(this.vcRef.element, 'program');
  }
}
