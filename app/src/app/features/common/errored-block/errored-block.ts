import { Component, inject, input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonDirective } from 'primeng/button';

@Component({
  selector: 'app-errored-block',
  imports: [ButtonDirective],
  templateUrl: './errored-block.html',
  styleUrl: './errored-block.scss',
})
export class ErroredBlock {
  public readonly message = input.required<string>();
  public readonly hint = input.required<string>();

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
