import { Component, computed, inject } from '@angular/core';
import { SheetLiveStore } from '../sheet-live.store';
import { SheetTakeoverRequest } from '../sheet-takeover-request/sheet-takeover-request';

/** The takeover requests waiting for the viewer's answer, oldest first. Takes no room when there are none. */
@Component({
  selector: 'app-sheet-takeover-requests',
  imports: [SheetTakeoverRequest],
  templateUrl: './sheet-takeover-requests.html',
  styleUrl: './sheet-takeover-requests.scss',
  host: { '[class.empty]': 'requests().length === 0' },
})
export class SheetTakeoverRequests {
  public readonly requests = computed(() => this.live.incoming());

  private readonly live = inject(SheetLiveStore);
}
