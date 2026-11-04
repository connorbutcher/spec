import { Component, input, output } from '@angular/core';
import { InstanceCountsUtil } from '../instance-counts.util';
import { InstanceCounts } from '../models/instance-counts.model';
import { InstanceField } from '../models/instance-field';
import { NumberField } from '../number-field/number-field';

/**
 * The "Added on the sheet" fields of a section or column block: the fewest copies, how many it starts
 * with and the most. Emits all three counts, nudged so fewest <= starts with <= most still holds.
 */
@Component({
  selector: 'app-instance-counts-fields',
  imports: [NumberField],
  templateUrl: './instance-counts-fields.html',
  styleUrl: './instance-counts-fields.scss',
})
export class InstanceCountsFields {
  /** Prefix for the input ids, e.g. "section" gives "section-min". */
  public readonly idPrefix = input.required<string>();

  public readonly counts = input.required<InstanceCounts>();

  public readonly disabled = input(false);

  public readonly changed = output<InstanceCounts>();

  public setCount(field: InstanceField, value: number | null): void {
    this.changed.emit(InstanceCountsUtil.withCount(this.counts(), field, value));
  }
}
