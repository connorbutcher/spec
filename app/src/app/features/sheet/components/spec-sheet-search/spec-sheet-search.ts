import {
  Component,
  computed,
  effect,
  inject,
  signal,
  Signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonDirective } from 'primeng/button';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { InputText } from 'primeng/inputtext';
import { Tooltip } from 'primeng/tooltip';
import { SheetFilterService } from '../../services/sheet-filter.service';

@Component({
  selector: 'app-spec-sheet-search',
  imports: [
    FormsModule,
    InputText,
    InputGroup,
    InputGroupAddon,
    Tooltip,
    ButtonDirective,
  ],
  templateUrl: './spec-sheet-search.html',
  styleUrl: './spec-sheet-search.scss',
})
export class SpecSheetSearch {
  public readonly matchCount = computed(
    () => this._filterService.matchReferenceIds().length,
  );
  public readonly filterSearchIndex = signal<number>(0);

  private readonly _filterService = inject(SheetFilterService);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);

  constructor() {
    effect(() => {
      const matches = this._filterService.matches();
      if (matches.length === 0) {
        this.filterSearchIndex.set(0);

        if (this._filterService.activeFilterResultReferenceId()) {
          this._filterService.activeFilterResultReferenceId.set(undefined);
          this.navigate(undefined);
        }

        return;
      }

      this.goTo(0);
    });
  }

  public goTo(index: number): void {
    if (index < 0) {
      return;
    }

    const matches = this._filterService.matches();
    if (matches.length === 0 || index >= matches.length) {
      return;
    }

    const match = matches[index];
    if (!match) {
      return;
    }

    this.filterSearchIndex.set(index);
    this._filterService.activeFilterResultReferenceId.set(match.referenceId);
    this._router.navigate([], {
      relativeTo: this._activatedRoute,
      fragment: match.referenceId,
      queryParamsHandling: 'merge',
    });
  }

  public reset(): void {
    this._filterService.filter.set('');
  }

  public get filter(): Signal<string> {
    return this._filterService.filter;
  }

  private navigate(fragment: string | undefined): void {
    this._router.navigate([], {
      relativeTo: this._activatedRoute,
      fragment,
      queryParamsHandling: 'merge',
    });
  }
}
