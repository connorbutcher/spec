import { Directive, ViewContainerRef } from '@angular/core';

@Directive({
  selector: '[sidePanelContent]',
  standalone: true,
})
export class SidePanelContentDirective {
  constructor(public viewContainerRef: ViewContainerRef) {}
}
