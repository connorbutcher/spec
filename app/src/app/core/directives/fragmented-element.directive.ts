import { Directive, ElementRef, input } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { combineLatest, filter } from 'rxjs';
import { FragmentHandler } from '../services/fragment-handler.service';

@Directive({
  selector: '[appFragmentedElement]',
})
export class FragmentedElementDirective {
  public readonly id = input<string>();

  constructor(
    { nativeElement }: ElementRef<HTMLElement>,
    private readonly _fragmentHandler: FragmentHandler,
  ) {
    combineLatest([
      toObservable(this.id),
      toObservable(this._fragmentHandler.activeFragment),
    ])
      .pipe(
        takeUntilDestroyed(),
        filter(([id, activeFragment]) => !!id && !!activeFragment),
      )
      .subscribe(([id, activeFragment]) => {
        const fragment = activeFragment!.fragment;

        if (id === fragment) {
          this._fragmentHandler.activeFragment.set(undefined);

          // Wait a tick to allow the layout to stabilize before scrolling
          requestAnimationFrame(() => {
            nativeElement.scrollIntoView({
              behavior: 'smooth',
              block: 'center',
            });
          });
        }
      });
  }
}
