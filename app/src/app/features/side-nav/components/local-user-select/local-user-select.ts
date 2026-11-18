import { Component, computed, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Select } from 'primeng/select';
import { Tooltip } from 'primeng/tooltip';
import { LocalUserService } from '../../../../core/services/local-user.service';

/** Picks which local user this tab acts as, while auth is disabled for local running. */
@Component({
  selector: 'app-local-user-select',
  imports: [FormsModule, Select, Tooltip],
  templateUrl: './local-user-select.html',
  styleUrl: './local-user-select.scss',
})
export class LocalUserSelect {
  public readonly isExpanded = input.required<boolean>();
  public readonly selectedLabel = computed(
    () =>
      this._localUserService.options.find(
        (option) => option.value === this._localUserService.selected(),
      )?.label ?? '',
  );

  private readonly _localUserService = inject(LocalUserService);

  public select(value: number): void {
    this._localUserService.select(value);
  }

  public get isEnabled(): boolean {
    return this._localUserService.isEnabled;
  }

  public get options() {
    return this._localUserService.options;
  }

  public get selected() {
    return this._localUserService.selected;
  }
}
