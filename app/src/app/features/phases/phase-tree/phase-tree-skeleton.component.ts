import { Component } from '@angular/core';

/** Placeholder rows shown while the phases load. */
@Component({
  selector: 'app-phase-tree-skeleton',
  template: `
    <div class="skeleton" aria-busy="true" aria-label="Loading phases">
      @for (width of widths; track $index) {
        <span class="row" [style.width.%]="width"></span>
      }
    </div>
  `,
  styles: `
    .skeleton {
      display: flex;
      flex-direction: column;
      gap: 10px;
      padding: 4px;

      .row {
        display: block;
        height: 14px;
        border-radius: 4px;
        background: linear-gradient(90deg, #e2e8f0 25%, #f1f5f9 50%, #e2e8f0 75%);
        background-size: 200% 100%;
        animation: shimmer 1.2s infinite linear;
      }
    }

    @keyframes shimmer {
      from {
        background-position: 200% 0;
      }
      to {
        background-position: -200% 0;
      }
    }

    @media (prefers-reduced-motion: reduce) {
      .skeleton .row {
        animation: none;
      }
    }
  `,
})
export class PhaseTreeSkeletonComponent {
  public readonly widths = [70, 55, 60, 50, 65, 40];
}
