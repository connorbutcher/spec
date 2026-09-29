import { Component, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { SettingField } from '../models/setting-field.model';
import { SettingValue } from '../models/setting-value';
import { SettingControl } from '../setting-control/setting-control';

/**
 * One setting on a line: its label, its control and, when it can be overridden, whether it is and a
 * button to go back to the default.
 */
@Component({
  selector: 'app-setting-row',
  imports: [ButtonModule, SettingControl, TooltipModule],
  templateUrl: './setting-row.html',
  styleUrl: './setting-row.scss',
  host: {
    '[class.overridden]': 'overridden()',
  },
})
export class SettingRow {
  public readonly field = input.required<SettingField>();
  /** The value the cell or type uses. */
  public readonly value = input.required<SettingValue>();
  public readonly inputId = input.required<string>();
  /** True on a cell, where the value can differ from its cell type's default. */
  public readonly overridable = input(false);
  public readonly overridden = input(false);
  /** The cell type's default, as text, shown while the value is overridden. */
  public readonly defaultText = input('');
  public readonly disabled = input(false);

  public readonly changed = output<SettingValue>();
  public readonly reset = output<void>();
}
