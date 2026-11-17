import { Component, inject, input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-info-block',
  templateUrl: './info-block.html',
  styleUrl: './info-block.scss',
})
export class InfoBlock {
  public readonly icon = input.required<string>();
  public readonly message = input.required<string>();
  public readonly hint = input<string>();

  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);

  public reload(): void {
    window.location.reload();
  }

  public goBack(): void {
    this._router.navigate(['..'], {
      relativeTo: this._activatedRoute,
      queryParamsHandling: 'preserve',
    });
  }
}
