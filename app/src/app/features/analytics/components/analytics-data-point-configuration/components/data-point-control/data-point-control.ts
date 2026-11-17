import { Component, inject, input, output } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonDirective } from 'primeng/button';
import { Tooltip } from 'primeng/tooltip';
import { DatasetColumn } from '../../../../../../core/models/analytics/dataset-column';

@Component({
  selector: 'app-data-point-control',
  imports: [ButtonDirective, Tooltip],
  templateUrl: './data-point-control.html',
  styleUrl: './data-point-control.scss',
})
export class DataPointControl {
  public readonly dataPoint = input.required<DatasetColumn>();
  public readonly dataPointRemoved = output<DatasetColumn>();

  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);

  public goToCell(): void {
    this._router.navigate([], {
      queryParams: { focusFragment: true },
      fragment: this.dataPoint().columnKey,
      relativeTo: this._activatedRoute,
    });
  }

  public deleteDataPoint(): void {
    this.dataPointRemoved.emit(this.dataPoint());
  }
}
