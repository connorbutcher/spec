import { Component, computed, inject, input } from '@angular/core';
import { PanelModule } from 'primeng/panel';
import { SettingsGroupState } from '../settings-group-state';

/**
 * A titled group of fields in the settings panel that folds away, as a toggleable PrimeNG panel.
 * Groups with the same `key` fold together, and stay folded from one item to the next.
 */
@Component({
  selector: 'app-settings-group',
  imports: [PanelModule],
  templateUrl: './settings-group.html',
  styleUrl: './settings-group.scss',
})
export class SettingsGroup {
  public readonly title = input.required<string>();
  /** What the group's fold state is remembered under; defaults to the title. */
  public readonly key = input<string>();
  /** A quiet note after the title, such as "2 changed". */
  public readonly note = input<string | null>(null);

  public readonly stateKey = computed(() => this.key() ?? this.title());

  private readonly state = inject(SettingsGroupState);

  public isFolded(): boolean {
    return this.state.isFolded(this.stateKey());
  }

  public setFolded(folded: boolean | undefined): void {
    this.state.setFolded(this.stateKey(), folded ?? false);
  }
}
