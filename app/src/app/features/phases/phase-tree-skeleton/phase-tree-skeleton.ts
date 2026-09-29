import { Component } from '@angular/core';
import { SkeletonModule } from 'primeng/skeleton';

/** Placeholder rows shown while the phases load. */
@Component({
  selector: 'app-phase-tree-skeleton',
  imports: [SkeletonModule],
  templateUrl: './phase-tree-skeleton.html',
  styleUrl: './phase-tree-skeleton.scss',
})
export class PhaseTreeSkeleton {
  public readonly widths = ['70%', '55%', '60%', '50%', '65%', '40%'];
}
