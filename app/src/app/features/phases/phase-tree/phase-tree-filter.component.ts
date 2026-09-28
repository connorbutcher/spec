import { Component, model } from '@angular/core';

/** The search box above the phase tree. */
@Component({
  selector: 'app-phase-tree-filter',
  template: `
    <label class="tree-filter">
      <span class="visually-hidden">Filter phases</span>
      <i class="pi pi-search" aria-hidden="true"></i>
      <input
        type="search"
        placeholder="Filter phases"
        autocomplete="off"
        [value]="query()"
        (input)="onInput($event)"
      />
    </label>
  `,
  styleUrl: './phase-tree-filter.component.scss',
})
export class PhaseTreeFilterComponent {
  public readonly query = model('');

  public onInput(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }
}
