import { Component, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-destroyable',
  template: '',
  standalone: true,
})
export class Destroyable implements OnDestroy {
  protected readonly destroy = new Subject<void>();

  ngOnDestroy(): void {
    this.destroy.next();
    this.destroy.complete();
  }
}
